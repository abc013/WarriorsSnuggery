using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Linq;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Maps.Pieces;
using WarriorsSnuggery.Objects;
using WarriorsSnuggery.Objects.Actors;
using WarriorsSnuggery.UI.Objects;
using WarriorsSnuggery.UI.Objects.Editor;

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

		readonly UIText mousePosition;
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
			Title.Position += new UIPos(0, -7120);

			mousePosition = new UIText(FontManager.Default, TextOffset.RIGHT) { Position = new UIPos(Right - 1024, -7172) };
			Add(mousePosition);

			if (game.MapType.OverridePiece != null)
			{
				var pieceName = new UIText(FontManager.Default, TextOffset.RIGHT) { Position = new UIPos(Right - 1024, -7684) };
				pieceName.SetText("Piece name: " + Color.Green + game.MapType.OverridePiece.File);
				Add(pieceName);
			}

			save = new Button("Save", "wooden", savePiece) { Position = new UIPos(Right - 2048, -5120) };

			var checkBoxPosition = new UIPos(Right - 1024, -6144);

			showNone = new CheckBox("wooden", true, (b) => deselectBoxes(Selected.NONE)) { Position = checkBoxPosition - new UIPos(736 * 3, 0) };
			Add(showNone);

			showTiles = new CheckBox("terrain_editor", onTicked: (b) => deselectBoxes(Selected.TILE)) { Position = checkBoxPosition - new UIPos(736 * 2, 0) };
			Add(showTiles);

			showActors = new CheckBox("actor_editor", onTicked: (b) => deselectBoxes(Selected.ACTOR)) { Position = checkBoxPosition - new UIPos(736 * 1, 0) };
			Add(showActors);

			showWalls = new CheckBox("wall_editor", onTicked: (b) => deselectBoxes(Selected.WALL)) { Position = checkBoxPosition };
			Add(showWalls);

			var widgetPosition = new UIPos(Right - 2048, -3584);

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
			return mouse.X > Right - 4096 - 128 && mouse.X < Right - 64 * Settings.EdgeScrolling;
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
					if (actorWidget.Rasterization && !CursorOnUI())
					{
						var pos = rasterizedPosition(MouseInput.GamePosition);
						ColorManager.DrawQuad(UIPos.FromGameCoordinates(pos), 64, Color.Blue);
					}

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
			mousePosition.SetText($"{(mouseInWorld ? Color.White : Color.Red)}{MouseInput.GamePosition.ToMPos()}{(mouseInWorld ? Color.Grey : new Color(1f, 0.75f, 0.75f))} | {MouseInput.GamePosition}");

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

		public override void KeyDown(Keys key, bool isControl, bool isShift, bool isAlt)
		{
			base.KeyDown(key, isControl, isShift, isAlt);

			switch (currentSelected)
			{
				case Selected.ACTOR:
					actorWidget.KeyDown(key, isControl, isShift, isAlt);
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
						var remove = sector.Actors.FirstOrDefault(a => !a.IsPlayer && (a.Position - MouseInput.GamePosition).SquaredFlatDist < 512 * 512);
						if (remove != null)
						{
							remove.Dispose();
							return;
						}
					}
					break;
				case Selected.WALL:
					var bounds = game.World.Map.Bounds;

					var mpos = MouseInput.GamePosition.ToMPos();
					var wpos = new WPos(mpos, wallWidget.Horizontal);

					if (!game.World.IsInWorld(wpos))
						return;

					game.World.WallLayer.Remove(wpos);
					break;
			}
		}

		void place()
		{
			if ((currentSelected != Selected.WALL && (currentSelected != Selected.ACTOR || !actorWidget.Rasterization)) && !game.World.IsInWorld(MouseInput.GamePosition))
				return;

			var pos = MouseInput.GamePosition;
			var mpos = pos.ToMPos();
			var bounds = game.World.Map.Bounds;

			switch (currentSelected)
			{
				case Selected.ACTOR:
					if (actorWidget.CurrentType == null)
						return;

					if (actorWidget.RelativeHP == 0)
						return;

					pos = actorWidget.Rasterization ? rasterizedPosition(pos) : pos;

					var team = Math.Max(actorWidget.Team, (byte)0);
					var actor = ActorCache.Create(game.World, actorWidget.CurrentType, pos, team, actorWidget.Bot, false);
					if (actor.Health != null)
						actor.Health.RelativeHP = actorWidget.RelativeHP;
					actor.Angle = actorWidget.RelativeFacing * Angle.MaxRange;

					game.World.Add(actor);
					break;
				case Selected.TILE:
					if (terrainWidget.CurrentType == null)
						return;

					if (!game.World.IsInWorld(mpos))
						return;

					if (game.World.TerrainLayer.Terrain[mpos.X, mpos.Y].Type == terrainWidget.CurrentType)
						return;

					var terrain = TerrainCache.Create(game.World, mpos, terrainWidget.CurrentType.ID);
					game.World.TerrainLayer.Set(terrain);
					game.World.TerrainLayer.CheckBordersAround(mpos);

					break;
				case Selected.WALL:
					if (wallWidget.CurrentType == null)
						return;

					var wpos = new WPos(mpos, wallWidget.Horizontal);

					if (!game.World.IsInWorld(wpos))
						return;

					var type = wallWidget.CurrentType;

					var plannedHealth = (int)(type.Health * wallWidget.RelativeHP);
					if (plannedHealth == 0 && type.Health != 0)
						return;

					var currentWall = game.World.WallLayer.Walls[wpos.X, wpos.Y];
					if (currentWall != null && currentWall.Type.ID == type.ID && currentWall.Health == plannedHealth)
						return;

					var wall = WallCache.Create(wpos, game.World, type.ID);
					wall.Health = plannedHealth;

					game.World.WallLayer.Set(wall);
					break;
			}
		}

		CPos rasterizedPosition(CPos pos)
		{
			pos += new CPos(Constants.TileSize / 4, Constants.TileSize / 4, 0);
			return new CPos(pos.X - pos.X % (Constants.TileSize / 2), pos.Y - pos.Y % (Constants.TileSize / 2), 0);
		}

		void savePiece()
		{
			var packageFile = game.MapType.OverridePiece;
			PieceSaver.SaveWorld(game.World, FileExplorer.FindPath(packageFile.Package.PiecesDirectory, packageFile.File, ".yaml"), packageFile.File);
			PieceManager.ReloadPiece(packageFile);
			game.AddInfoMessage(150, "Map saved!");
		}
	}
}
