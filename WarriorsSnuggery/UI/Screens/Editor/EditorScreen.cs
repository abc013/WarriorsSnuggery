using System;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;
using WarriorsSnuggery.UI.Objects;

namespace WarriorsSnuggery.UI.Screens
{
	public class EditorScreen : Screen
	{
		readonly CheckBox showNone;
		readonly CheckBox showTiles;
		readonly CheckBox showActors;
		readonly CheckBox showWalls;

		readonly TerrainEditorWidget terrainWidget;
		readonly ActorEditorWidget actorWidget;
		readonly WallEditorWidget wallWidget;

		readonly UITextLine mousePosition;
		readonly Button save;

		readonly Game game;

		enum Selected
		{
			TILE,
			ACTOR,
			WALL,
			NONE
		}

		Selected currentSelected = Selected.NONE;

		public EditorScreen(Game game) : base("Editor", 0)
		{
			this.game = game;
			Title.Position += new CPos(0, -7120, 0);

			mousePosition = new UITextLine(new CPos((int)(WindowInfo.UnitWidth * 512 - 1024), -7172, 0), FontManager.Pixel16, TextOffset.RIGHT);
			Content.Add(mousePosition);

			save = new Button(new CPos((int)(WindowInfo.UnitWidth * 512 - 2048), -5120, 0), "Save", "wooden", savePiece);

			var checkBoxPosition = new CPos((int)(WindowInfo.UnitWidth * 512) - 1024, -6144, 0);

			showNone = CheckBoxCreator.Create("wooden", checkBoxPosition - new CPos(736 * 3, 0, 0), false, (b) => deselectBoxes(Selected.NONE));
			showNone.Checked = true;
			Content.Add(showNone);

			showTiles = CheckBoxCreator.Create("terrain_editor", checkBoxPosition - new CPos(736 * 2, 0, 0), false, (b) => deselectBoxes(Selected.TILE));
			Content.Add(showTiles);

			showActors = CheckBoxCreator.Create("actor_editor", checkBoxPosition - new CPos(736, 0, 0), false, (b) => deselectBoxes(Selected.ACTOR));
			Content.Add(showActors);

			showWalls = CheckBoxCreator.Create("wall_editor", checkBoxPosition, false, (b) => deselectBoxes(Selected.WALL));
			Content.Add(showWalls);

			var widgetPosition = new CPos((int)(WindowInfo.UnitWidth * 512) - 2048, -3584, 0);

			terrainWidget = new TerrainEditorWidget(widgetPosition);
			actorWidget = new ActorEditorWidget(widgetPosition);
			wallWidget = new WallEditorWidget(widgetPosition);
		}

		void deselectBoxes(Selected selected)
		{
			showNone.Checked = selected == Selected.NONE;
			showActors.Checked = selected == Selected.ACTOR;
			showTiles.Checked = selected == Selected.TILE;
			showWalls.Checked = selected == Selected.WALL;
			currentSelected = selected;
		}

		public override bool CursorOnUI()
		{
			if (currentSelected == Selected.NONE)
				return false;

			var mouse = MouseInput.WindowPosition;
			return mouse.X > WindowInfo.UnitWidth * 512 - 4096 - 128 && mouse.X < WindowInfo.UnitWidth * 512 - 64 && mouse.Y < -2048 + 4096 + 512 && mouse.Y > -6120 - 4096 + 512;
		}

		public override void Hide()
		{
			terrainWidget.DisableTooltip();
			actorWidget.DisableTooltip();
			wallWidget.DisableTooltip();
		}

		public override void Render()
		{
			base.Render();

			if (game.Type == GameType.EDITOR)
				save.Render();

			switch (currentSelected)
			{
				case Selected.TILE:
					terrainWidget.Render();
					break;
				case Selected.ACTOR:
					actorWidget.Render();
					break;
				case Selected.WALL:
					wallWidget.Render();
					break;
			}
		}

		public override void DebugRender()
		{
			base.DebugRender();

			switch (currentSelected)
			{
				case Selected.TILE:
					terrainWidget.DebugRender();
					break;
				case Selected.ACTOR:
					actorWidget.DebugRender();
					break;
				case Selected.WALL:
					wallWidget.DebugRender();
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

			mousePosition.WriteText(Color.White + "" + MouseInput.GamePosition.ToMPos() + Color.Grey + " | " + MouseInput.GamePosition);

			switch (currentSelected)
			{
				case Selected.TILE:
					terrainWidget.Tick();
					break;
				case Selected.ACTOR:
					actorWidget.Tick();
					break;
				case Selected.WALL:
					wallWidget.Tick();
					break;
			}
		}

		void remove()
		{
			var removeSectors = game.World.ActorLayer.GetSectors(MouseInput.GamePosition, 512);
			foreach (var sector in removeSectors)
			{
				var remove = sector.Actors.Find(a => !a.IsPlayer && (a.Position - MouseInput.GamePosition).SquaredFlatDist < 512 * 512);
				if (remove != null)
				{
					remove.Dispose();
					return;
				}
			}

			var bounds = game.World.Map.Bounds;
			var pos4 = MouseInput.GamePosition.ToMPos();
			pos4 = new MPos(pos4.X < 0 ? 0 : pos4.X, pos4.Y < 0 ? 0 : pos4.Y);
			pos4 = new MPos(pos4.X > bounds.X ? bounds.X : pos4.X, pos4.Y > bounds.Y ? bounds.Y : pos4.Y);
			pos4 = new MPos(pos4.X * 2 + (wallWidget.Horizontal ? 0 : 1), pos4.Y);

			var wallLayer = game.World.WallLayer;
			if (pos4.X >= wallLayer.Bounds.X)
				pos4 = new MPos(wallLayer.Bounds.X - 1, pos4.Y);
			if (pos4.Y >= wallLayer.Bounds.Y)
				pos4 = new MPos(pos4.X, wallLayer.Bounds.Y - 1);

			wallLayer.Remove(pos4);
		}

		void place()
		{
			if (!game.World.IsInWorld(MouseInput.GamePosition))
				return;

			var pos = MouseInput.GamePosition;
			pos = actorWidget.Rasterization ? new CPos(pos.X - pos.X % 512, pos.Y - pos.Y % 512, 0) : pos;
			var mpos = MouseInput.GamePosition.ToMPos();
			mpos = new MPos(mpos.X < 0 ? 0 : mpos.X, mpos.Y < 0 ? 0 : mpos.Y);

			switch (currentSelected)
			{
				case Selected.ACTOR:
					if (actorWidget.CurrentType == null)
						return;

					var team = Math.Clamp(actorWidget.Team, (byte)0, Settings.MaxTeams);
					var actor = ActorCreator.Create(game.World, actorWidget.CurrentType, pos, team, actorWidget.Bot, false, actorWidget.RelativeHP);
					actor.Angle = actorWidget.RelativeFacing * 2 * (float)Math.PI;

					game.World.Add(actor);
					break;
				case Selected.TILE:
					if (terrainWidget.CurrentType == null)
						return;

					if (game.World.TerrainLayer.Terrain[mpos.X, mpos.Y].Type == terrainWidget.CurrentType)
						return;

					var terrain = TerrainCreator.Create(game.World, mpos, terrainWidget.CurrentType.ID);
					game.World.TerrainLayer.Set(terrain);

					WorldRenderer.CheckTerrainAround(mpos, true);

					break;
				case Selected.WALL:
					if (wallWidget.CurrentType == null)
						return;

					var bounds = game.World.Map.Bounds;
					if (mpos.X > bounds.X || mpos.Y > bounds.Y)
						return;

					mpos = new MPos(mpos.X * 2 + (wallWidget.Horizontal ? 0 : 1), mpos.Y);

					var type = wallWidget.CurrentType;

					var wallLayer = game.World.WallLayer;

					var plannedHealth = (int)(type.Health * wallWidget.RelativeHP);

					var currentWall = wallLayer.Walls[mpos.X, mpos.Y];
					if (currentWall != null && currentWall.Type.ID == type.ID && currentWall.Health == plannedHealth)
						return;

					var wall = WallCreator.Create(mpos, wallLayer, type.ID);
					wall.Health = plannedHealth;

					wallLayer.Set(wall);
					break;
			}
		}

		void savePiece()
		{
			game.World.Save(FileExplorer.FindPath(FileExplorer.Maps, game.MapType.OverridePiece, ".yaml"), game.MapType.OverridePiece, false);
			Maps.PieceManager.ReloadPiece(game.MapType.OverridePiece);
			game.AddInfoMessage(150, "Map saved!");
		}
	}
}
