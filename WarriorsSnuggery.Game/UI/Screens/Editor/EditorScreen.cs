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

			mousePosition = new UITextLine(FontManager.Pixel16, TextOffset.RIGHT) { Position = new CPos((int)(WindowInfo.UnitWidth * 512 - 1024), -7172, 0) };
			Content.Add(mousePosition);

			if (!string.IsNullOrEmpty(game.MapType.OverridePiece))
			{
				var pieceName = new UITextLine(FontManager.Pixel16, TextOffset.RIGHT) { Position = new CPos((int)(WindowInfo.UnitWidth * 512 - 1024), -7684, 0) };
				pieceName.WriteText("Piece name: " + Color.Green + game.MapType.OverridePiece);
				Content.Add(pieceName);
			}

			save = new Button("Save", "wooden", savePiece) { Position = new CPos((int)(WindowInfo.UnitWidth * 512 - 2048), -5120, 0) };

			var checkBoxPosition = new CPos((int)(WindowInfo.UnitWidth * 512) - 1024, -6144, 0);

			showNone = new CheckBox("wooden", true, (b) => deselectBoxes(Selected.NONE)) { Position = checkBoxPosition - new CPos(736 * 3, 0, 0) };
			Content.Add(showNone);

			showTiles = new CheckBox("terrain_editor", onTicked: (b) => deselectBoxes(Selected.TILE)) { Position = checkBoxPosition - new CPos(736 * 2, 0, 0) };
			Content.Add(showTiles);

			showActors = new CheckBox("actor_editor", onTicked: (b) => deselectBoxes(Selected.ACTOR)) { Position = checkBoxPosition - new CPos(736 * 1, 0, 0) };
			Content.Add(showActors);

			showWalls = new CheckBox("wall_editor", onTicked: (b) => deselectBoxes(Selected.WALL)) { Position = checkBoxPosition };
			Content.Add(showWalls);

			var widgetPosition = new CPos((int)(WindowInfo.UnitWidth * 512) - 2048, -3584, 0);

			terrainWidget = new TerrainEditorWidget() { Position = widgetPosition };
			actorWidget = new ActorEditorWidget() { Position = widgetPosition };
			wallWidget = new WallEditorWidget() { Position = widgetPosition };
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
			return mouse.X > WindowInfo.UnitWidth * 512 - 4096 - 128 && mouse.X < WindowInfo.UnitWidth * 512 - 64 * Settings.EdgeScrolling;
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
				default:
					if (game.InteractionMode == InteractionMode.EDITOR)
						save.Render();

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
				default:
					if (game.InteractionMode == InteractionMode.EDITOR)
						save.DebugRender();

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

			var mouseInWorld = game.World.IsInWorld(MouseInput.GamePosition);
			mousePosition.WriteText((mouseInWorld ? Color.White : Color.Red) + "" + (MouseInput.GamePosition + game.World.Map.TopLeftCorner).ToMPos() + (mouseInWorld ? Color.Grey : new Color(1f, 0.75f, 0.75f)) + " | " + MouseInput.GamePosition);

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
				default:
					if (game.InteractionMode == InteractionMode.EDITOR)
						save.Tick();

					break;
			}
		}

		void remove()
		{
			switch (currentSelected)
			{
				case Selected.ACTOR:
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
					break;
				case Selected.WALL:
					var bounds = game.World.Map.Bounds;
					var pos = MouseInput.GamePosition.ToMPos();

					if (pos.X < 0 || pos.Y < 0 || pos.X > bounds.X || pos.Y > bounds.Y)
						return;

					pos = new MPos(pos.X * 2 + (wallWidget.Horizontal ? 0 : 1), pos.Y);

					game.World.WallLayer.Remove(pos);
					break;
			}
		}

		void place()
		{
			if (currentSelected != Selected.WALL && !game.World.IsInWorld(MouseInput.GamePosition))
				return;

			var pos = MouseInput.GamePosition;
			var mpos = (MouseInput.GamePosition + game.World.Map.TopLeftCorner).ToMPos();
			var bounds = game.World.Map.Bounds;

			switch (currentSelected)
			{
				case Selected.ACTOR:
					if (actorWidget.CurrentType == null)
						return;

					if (actorWidget.RelativeHP == 0)
						return;

					pos = actorWidget.Rasterization ? new CPos(pos.X - pos.X % 512, pos.Y - pos.Y % 512, 0) : pos;

					var team = Math.Clamp(actorWidget.Team, (byte)0, Settings.MaxTeams);
					var actor = ActorCreator.Create(game.World, actorWidget.CurrentType, pos, team, actorWidget.Bot, false, actorWidget.RelativeHP);
					actor.Angle = actorWidget.RelativeFacing * Angle.MaxRange;

					game.World.Add(actor);
					break;
				case Selected.TILE:
					if (terrainWidget.CurrentType == null)
						return;

					if (mpos.X < 0 || mpos.Y < 0 || mpos.X >= bounds.X || mpos.Y >= bounds.Y)
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

					if (mpos.X < 0 || mpos.Y < 0 || mpos.X > bounds.X || mpos.Y > bounds.Y)
						return;

					mpos = new MPos(mpos.X * 2 + (wallWidget.Horizontal ? 0 : 1), mpos.Y);

					var wallLayer = game.World.WallLayer;

					if (mpos.X >= wallLayer.Bounds.X - 2)
						return;

					if (mpos.Y >= wallLayer.Bounds.Y - 2 && wallWidget.Horizontal)
						return;

					var type = wallWidget.CurrentType;

					var plannedHealth = (int)(type.Health * wallWidget.RelativeHP);
					if (plannedHealth == 0 && type.Health != 0)
						return;

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
