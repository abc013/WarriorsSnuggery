/*
 * User: Andreas
 * Date: 21.10.2018
 * Time: 21:18
 */
using WarriorsSnuggery.Objects;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.UI
{
	public class EditorScreen : Screen
	{
		readonly CheckBox showTiles;
		readonly CheckBox showActors;
		readonly CheckBox showWalls;

		readonly PanelList tiles;
		readonly PanelList actors;
		readonly PanelList walls;

		readonly CheckBox wallBox;
		readonly CheckBox rasterizationBox;
		readonly CheckBox isBot;
		readonly TextBox team;

		readonly TextLine mousePosition;
		readonly Button save;
		readonly TextLine saved;
		int savedTick;

		readonly Game game;

		enum Selected
		{
			TILE,
			ACTOR,
			WALL,
			NONE
		}
		
		Selected currentSelected = Selected.NONE;
		TerrainType terrainSelected;
		ActorType actorSelected;
		WallType wallSelected;
		bool horizontal;

		public EditorScreen(Game game) : base("Editor", 0)
		{
			this.game = game;
			Title.Position += new CPos(0,-7120,0);

			mousePosition = new TextLine(new CPos((int)(WindowInfo.UnitWidth * 512 - 2560), -7172, 0), IFont.Pixel16);
			save = ButtonCreator.Create("wooden", new CPos((int) (WindowInfo.UnitWidth * 512 - 2048), -5120, 0), "Save", savePiece);
			saved = new TextLine(new CPos((int)(WindowInfo.UnitWidth * 512 - 2048), -5120, 0), IFont.Pixel16, TextLine.OffsetType.MIDDLE);
			saved.SetText("Save");

			showTiles = CheckBoxCreator.Create("terrain_editor", new CPos((int) (WindowInfo.UnitWidth * 512 - 2496),-2536,0), false, (b) =>
			{
				deselectBoxes(Selected.TILE);
			});

			showActors = CheckBoxCreator.Create("actor_editor", new CPos((int) (WindowInfo.UnitWidth * 512 - 1760),-2536,0), false, (b) =>
			{
				deselectBoxes(Selected.ACTOR);
			});

			showWalls = CheckBoxCreator.Create("wall_editor", new CPos((int) (WindowInfo.UnitWidth * 512 - 1024),-2536,0), false, (b) =>
			{
				deselectBoxes(Selected.WALL);
			});

			tiles = new PanelList(new CPos((int) (WindowInfo.UnitWidth * 512 - 2048), 2048, 0), new MPos(2048, 4096), new MPos(512,512), 4, "UI_wood1", "UI_wood3", "UI_wood2");
			foreach(var n in TerrainCreator.GetIDs())
			{
				var a = TerrainCreator.GetType(n);
				tiles.Add(new PanelItem(CPos.Zero, n + "", new ImageRenderable(a.Texture, 1f), new MPos(512,512), () => terrainSelected = a));
			}

			actors = new PanelList(new CPos((int) (WindowInfo.UnitWidth * 512 - 2048), 2048, 0), new MPos(2048,4096), new MPos(512,512), 4, "UI_wood1", "UI_wood3", "UI_wood2");
			foreach(var n in ActorCreator.GetNames())
			{
				var a = ActorCreator.GetType(n);
				actors.Add(new PanelItem(CPos.Zero, n, new ImageRenderable(new TextureInfo("questionmark", TextureType.IMAGE, 10, 12, 12, false).GetTextures()[0], 0.4f), new MPos(512,512), () => actorSelected = a));
			}

			walls = new PanelList(new CPos((int) (WindowInfo.UnitWidth * 512 - 2048), 2048, 0), new MPos(2048, 4096), new MPos(512,512), 4, "UI_wood1", "UI_wood3", "UI_wood2");
			foreach(var n in WallCreator.GetIDs())
			{
				var a = WallCreator.GetType(n);
				walls.Add(new PanelItem(CPos.Zero, n + "", new ImageRenderable(a.GetTexture(true)), new MPos(512,512), () => wallSelected = a));
			}

			wallBox = CheckBoxCreator.Create("wooden", new CPos((int) (WindowInfo.UnitWidth * 512 - 2048), 6244, 0), false, (b) => horizontal = b);
			rasterizationBox = CheckBoxCreator.Create("wooden", new CPos((int) (WindowInfo.UnitWidth * 512 - 2048), 6244, 0), false, (b) => {});
			isBot = CheckBoxCreator.Create("wooden", new CPos((int) (WindowInfo.UnitWidth * 512 - 1024), -4196, 0), false, (b) => {});
			team = TextBoxCreator.Create("wooden", new CPos((int) (WindowInfo.UnitWidth * 512 - 1024), -3372, 0), "0", 1, true);
		}

		void deselectBoxes(Selected selected)
		{
			showActors.Checked = selected == Selected.ACTOR;
			showTiles.Checked = selected == Selected.TILE;
			showWalls.Checked = selected == Selected.WALL;
			currentSelected = selected;
		}

		public override bool CursorOnUI()
		{
			var mouse = MouseInput.WindowPosition;
			return mouse.X > WindowInfo.UnitWidth * 512 - 4096 - 128;
		}

		public override void Render()
		{
			base.Render();

			if(game.Type == GameType.EDITOR)
				save.Render();

			if (savedTick > 0)
			{
				savedTick--;
				saved.Scale = 1.7f - savedTick / 15f;
				saved.SetColor(new Color(1f, 1f, 1f, savedTick / 15f));
				saved.Render();
			}

			mousePosition.Render();

			showTiles.Render();
			showActors.Render();
			showWalls.Render();

			switch (currentSelected)
			{
				case Selected.ACTOR:
					actors.Render();
					rasterizationBox.Render();
					isBot.Render();
					team.Render();
					break;
				case Selected.TILE:
					tiles.Render();
					break;
				case Selected.WALL:
					walls.Render();
					wallBox.Render();
					break;
			}
		}

		public override void Tick()
		{
			base.Tick();

			// Zoom function
			if (!CursorOnUI())
				Camera.Zoom(MouseInput.WheelState * 1.5f);

			// place something
			if (MouseInput.isLeftClicked)
				place();

			if(MouseInput.isLeftDown && (currentSelected == Selected.TILE || currentSelected == Selected.WALL))
				place();

			// Delete something
			if (MouseInput.isRightClicked)
				remove();

			if (game.Type == GameType.EDITOR)
				save.Tick();

			mousePosition.Tick();
			mousePosition.SetText(MouseInput.GamePosition);

			showTiles.Tick();
			showActors.Tick();
			showWalls.Tick();

			switch(currentSelected)
			{
				case Selected.ACTOR:
					actors.Tick();
					rasterizationBox.Tick();
					isBot.Tick();
					team.Tick();
					break;
				case Selected.TILE:
					tiles.Tick();
					break;
				case Selected.WALL:
					walls.Tick();
					wallBox.Tick();
					break;
			}
		}

		void remove()
		{
			var remove = game.World.Actors.Find(a => a.Position.DistToXY(MouseInput.GamePosition) < 512);
			if (remove != null)
			{
				remove.Dispose();
			}
			else
			{
				var remove2 = game.World.Objects.Find(a => a.Position.DistToXY(MouseInput.GamePosition) < 512);
				if (remove2 != null)
				{
					remove2.Dispose();
				}
				else
				{
					var pos4 = MouseInput.GamePosition.ToMPos();
					pos4 = new MPos(pos4.X < 0 ? 0 : pos4.X, pos4.Y < 0 ? 0 : pos4.Y);
					pos4 = new MPos(pos4.X > game.World.Map.Size.X ? game.World.Map.Size.X : pos4.X, pos4.Y > game.World.Map.Size.Y ? game.World.Map.Size.Y : pos4.Y);
					pos4 = new MPos(pos4.X * 2 + (horizontal ? 0 : 1), pos4.Y);

					if (pos4.X >= game.World.WallLayer.Size.X) pos4 = new MPos(game.World.WallLayer.Size.X - 1, pos4.Y);
					if (pos4.Y >= game.World.WallLayer.Size.Y) pos4 = new MPos(pos4.X, game.World.WallLayer.Size.Y - 1);
					game.World.WallLayer.Remove(pos4);
				}
			}
		}

		void place()
		{
			if (!game.World.IsInWorld(MouseInput.GamePosition))
				return;

			if (CursorOnUI())
				return;

			var pos  = MouseInput.GamePosition;
			pos = rasterizationBox.Checked ? new CPos(pos.X - pos.X % 512, pos.Y - pos.Y % 512, 0) : pos;
			var wpos = MouseInput.GamePosition.ToWPos();

			switch (currentSelected)
			{
				case Selected.ACTOR:
					if (actorSelected == null)
						return;

					game.World.Add(ActorCreator.Create(game.World, actorSelected, pos, int.Parse(team.Text), isBot.Checked));
					break;
				case Selected.TILE:
					if (terrainSelected == null)
						return;

					wpos = new WPos(wpos.X < 0 ? 0 : wpos.X, wpos.Y < 0 ? 0 : wpos.Y,0);
					var terrain = TerrainCreator.Create(game.World, wpos, terrainSelected.ID);
					Camera.UpdateView();

					game.World.TerrainLayer.Set(terrain);
					break;
				case Selected.WALL:
					if (wallSelected == null)
						return;

					wpos = new WPos(wpos.X < 0 ? 0 : wpos.X, wpos.Y < 0 ? 0 : wpos.Y,0);
					wpos = new WPos(wpos.X > game.World.Map.Size.X ? game.World.Map.Size.X : wpos.X, wpos.Y > game.World.Map.Size.Y ? game.World.Map.Size.Y : wpos.Y,0);
					wpos = new WPos(wpos.X * 2 + (horizontal ? 0 : 1), wpos.Y, 0);

					game.World.WallLayer.Set(WallCreator.Create(wpos, wallSelected.ID));
					break;
			}
		}

		void savePiece()
		{
			savedTick = 15;
			var keys = game.MapType.ImportantParts.Keys;
			foreach(var key in keys)
			{
				game.World.Map.Save(key);
				break;
			}
			game.AddInfoMessage(150, "Map saved!");
		}

		public override void Dispose()
		{
			base.Dispose();

			showTiles.Dispose();
			showActors.Dispose();
			showWalls.Dispose();

			tiles.Dispose();
			actors.Dispose();
			walls.Dispose();

			wallBox.Dispose();
			rasterizationBox.Dispose();
			isBot.Dispose();
			team.Dispose();
		}
	}
}
