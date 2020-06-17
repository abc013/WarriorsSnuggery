using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

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
		readonly TextLine wallText;
		readonly TextLine rasterizationText;
		readonly TextLine botText;
		readonly TextLine teamText;

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
			Title.Position += new CPos(0, -7120, 0);

			mousePosition = new TextLine(new CPos((int)(WindowInfo.UnitWidth * 512 - 1024), -7172, 0), FontManager.Pixel16, TextLine.OffsetType.RIGHT);
			save = ButtonCreator.Create("wooden", new CPos((int)(WindowInfo.UnitWidth * 512 - 2048), -5120, 0), "Save", savePiece);
			saved = new TextLine(new CPos((int)(WindowInfo.UnitWidth * 512 - 2048), -5120, 0), FontManager.Pixel16, TextLine.OffsetType.MIDDLE);
			saved.SetText("Save");

			showTiles = CheckBoxCreator.Create("terrain_editor", new CPos((int)(WindowInfo.UnitWidth * 512 - 2496), -2536, 0), false, (b) =>
			 {
				 deselectBoxes(Selected.TILE);
			 });

			showActors = CheckBoxCreator.Create("actor_editor", new CPos((int)(WindowInfo.UnitWidth * 512 - 1760), -2536, 0), false, (b) =>
			 {
				 deselectBoxes(Selected.ACTOR);
			 });

			showWalls = CheckBoxCreator.Create("wall_editor", new CPos((int)(WindowInfo.UnitWidth * 512 - 1024), -2536, 0), false, (b) =>
			 {
				 deselectBoxes(Selected.WALL);
			 });

			tiles = new PanelList(new CPos((int)(WindowInfo.UnitWidth * 512 - 2048), 2048, 0), new MPos(2048, 4096), new MPos(512, 512), PanelManager.Get("wooden"));
			foreach (var a in TerrainCreator.Types.Values)
				tiles.Add(new PanelItem(CPos.Zero, new BatchObject(a.Texture, Color.White), new MPos(512, 512), a.ID.ToString(), new string[0], () => terrainSelected = a));

			actors = new PanelList(new CPos((int)(WindowInfo.UnitWidth * 512 - 2048), 2048, 0), new MPos(2048, 4096), new MPos(512, 512), PanelManager.Get("wooden"));
			foreach (var pair in ActorCreator.Types)
			{
				var a = pair.Value;
				var sprite = a.GetPreviewSprite();
				var scale = (sprite.Width > sprite.Height ? 24f / sprite.Width : 24f / sprite.Height) - 0.1f;
				actors.Add(new PanelItem(CPos.Zero, new BatchObject(sprite, Color.White), new MPos(512, 512), a.Playable == null ? pair.Key : a.Playable.Name, new string[0], () => actorSelected = a)
				{
					Scale = scale
				});
			}

			walls = new PanelList(new CPos((int)(WindowInfo.UnitWidth * 512 - 2048), 2048, 0), new MPos(2048, 4096), new MPos(512, 1024), PanelManager.Get("wooden"));
			foreach (var a in WallCreator.Types.Values)
				walls.Add(new PanelItem(CPos.Zero, new BatchObject(a.GetTexture(true, 0), Color.White), new MPos(512, 512), a.ID.ToString(), new string[0], () => wallSelected = a));

			wallBox = CheckBoxCreator.Create("wooden", new CPos((int)(WindowInfo.UnitWidth * 512 - 2048), 6244, 0), false, (b) => horizontal = b);
			wallText = new TextLine(new CPos((int)(WindowInfo.UnitWidth * 512 - 2048), 6756, 0), FontManager.Pixel16, TextLine.OffsetType.MIDDLE);
			wallText.SetText("place vertical");
			rasterizationBox = CheckBoxCreator.Create("wooden", new CPos((int)(WindowInfo.UnitWidth * 512 - 2048), 6244, 0), false, (b) => { });
			rasterizationText = new TextLine(new CPos((int)(WindowInfo.UnitWidth * 512 - 2048), 6756, 0), FontManager.Pixel16, TextLine.OffsetType.MIDDLE);
			rasterizationText.SetText("align to grid");
			isBot = CheckBoxCreator.Create("wooden", new CPos((int)((WindowInfo.UnitWidth * 512) - 1024), -4196, 0), false, (b) => { });
			team = TextBoxCreator.Create("wooden", new CPos((int)((WindowInfo.UnitWidth * 512) - 1024), -3372, 0), "0", 1, true);
			team.OnEnter = () =>
			{
				if (team.Text == string.Empty)
					team.Text = "0";

				var num = byte.Parse(team.Text);
				if (num >= Settings.MaxTeams)
					team.Text = "" + (Settings.MaxTeams - 1);
			};
			teamText = new TextLine(new CPos((int)(WindowInfo.UnitWidth * 512 - 1536), -3372, 0), FontManager.Pixel16, TextLine.OffsetType.RIGHT);
			teamText.SetText("Team:");
			botText = new TextLine(new CPos((int)(WindowInfo.UnitWidth * 512 - 1536), -4196, 0), FontManager.Pixel16, TextLine.OffsetType.RIGHT);
			botText.SetText("Is bot:");
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
			return mouse.X > WindowInfo.UnitWidth * 512 - 4096 - 128 && mouse.X < WindowInfo.UnitWidth * 512 - 64;
		}

		public override void Hide()
		{
			tiles.DisableTooltip();
			actors.DisableTooltip();
			walls.DisableTooltip();
		}

		public override void Render()
		{
			base.Render();

			if (game.Type == GameType.EDITOR)
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
					rasterizationText.Render();
					isBot.Render();
					team.Render();
					botText.Render();
					teamText.Render();
					break;
				case Selected.TILE:
					tiles.Render();
					break;
				case Selected.WALL:
					walls.Render();
					wallBox.Render();
					wallText.Render();
					break;
			}
		}

		public override void Tick()
		{
			base.Tick();

			if (!CursorOnUI())
			{
				// Zoom function
				Camera.Zoom(MouseInput.WheelState);

				// place something
				if (MouseInput.IsLeftClicked)
					place();

				if (MouseInput.IsLeftDown && (currentSelected == Selected.TILE || currentSelected == Selected.WALL))
					place();

				// Delete something
				if (MouseInput.IsRightClicked || MouseInput.IsRightDown)
					remove();
			}

			if (game.Type == GameType.EDITOR)
				save.Tick();

			mousePosition.Tick();
			mousePosition.WriteText(Color.White + "" + MouseInput.GamePosition.ToMPos() + Color.Grey + " | " + MouseInput.GamePosition);

			showTiles.Tick();
			showActors.Tick();
			showWalls.Tick();

			switch (currentSelected)
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
			var remove = game.World.Actors.Find(a => (a.Position - MouseInput.GamePosition).FlatDist < 512);
			if (remove != null)
			{
				remove.Dispose();
			}
			else
			{
				var remove2 = game.World.Objects.Find(a => (a.Position - MouseInput.GamePosition).FlatDist < 512);
				if (remove2 != null)
				{
					remove2.Dispose();
				}
				else
				{
					var pos4 = MouseInput.GamePosition.ToMPos();
					pos4 = new MPos(pos4.X < 0 ? 0 : pos4.X, pos4.Y < 0 ? 0 : pos4.Y);
					pos4 = new MPos(pos4.X > game.World.Map.Bounds.X ? game.World.Map.Bounds.X : pos4.X, pos4.Y > game.World.Map.Bounds.Y ? game.World.Map.Bounds.Y : pos4.Y);
					pos4 = new MPos(pos4.X * 2 + (horizontal ? 0 : 1), pos4.Y);

					if (pos4.X >= game.World.WallLayer.Size.X) pos4 = new MPos(game.World.WallLayer.Size.X - 1, pos4.Y);
					if (pos4.Y >= game.World.WallLayer.Size.Y) pos4 = new MPos(pos4.X, game.World.WallLayer.Size.Y - 1);
					game.World.WallLayer.Remove(pos4);
				}
			}
		}

		void place()
		{
			if (!game.World.IsInWorld(MouseInput.GamePosition) && currentSelected != Selected.WALL)
				return;

			var pos = MouseInput.GamePosition;
			pos = rasterizationBox.Checked ? new CPos(pos.X - pos.X % 512, pos.Y - pos.Y % 512, 0) : pos;
			var mpos = MouseInput.GamePosition.ToMPos();

			switch (currentSelected)
			{
				case Selected.ACTOR:
					if (actorSelected == null)
						return;

					game.World.Add(ActorCreator.Create(game.World, actorSelected, pos, byte.Parse(team.Text), isBot.Checked));
					break;
				case Selected.TILE:
					if (terrainSelected == null)
						return;

					mpos = new MPos(mpos.X < 0 ? 0 : mpos.X, mpos.Y < 0 ? 0 : mpos.Y);
					var terrain = TerrainCreator.Create(game.World, mpos, terrainSelected.ID);
					game.World.TerrainLayer.Set(terrain);

					WorldRenderer.CheckTerrainAround(mpos, true);

					break;
				case Selected.WALL:
					if (wallSelected == null)
						return;

					mpos = new MPos(mpos.X < 0 ? 0 : mpos.X, mpos.Y < 0 ? 0 : mpos.Y);
					if (mpos.X > game.World.Map.Bounds.X || mpos.Y > game.World.Map.Bounds.Y)
						return;

					mpos = new MPos(mpos.X * 2 + (horizontal ? 0 : 1), mpos.Y);

					if (game.World.WallLayer.Walls[mpos.X, mpos.Y] != null && game.World.WallLayer.Walls[mpos.X, mpos.Y].Type.ID == wallSelected.ID)
						return;

					game.World.WallLayer.Set(WallCreator.Create(mpos, game.World.WallLayer, wallSelected.ID));
					break;
			}
		}

		void savePiece()
		{
			savedTick = 15;
			game.World.Map.Save(FileExplorer.FindPath(FileExplorer.Maps, game.MapType.OverridePiece, ".yaml"), game.MapType.OverridePiece);
			Maps.PieceManager.RefreshPiece(game.MapType.OverridePiece);
			game.AddInfoMessage(150, "Map saved!");
		}
	}
}
