using OpenTK.Windowing.GraphicsLibraryFramework;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Maps;
using WarriorsSnuggery.Objects;
using WarriorsSnuggery.UI.Objects;

namespace WarriorsSnuggery.UI.Screens
{
	public class PieceSelectionScreen : Screen
	{
		readonly Game game;

		readonly PanelList mapSelection;

		readonly CreatePieceScreen createPieceScreen;

		public PieceSelectionScreen(Game game) : base("Piece Selection")
		{
			this.game = game;
			Title.Position = new CPos(0, -4096, 0);

			mapSelection = new PanelList(new MPos(4096, 4096), new MPos(512, 512), "wooden") { Position = new CPos(0, 1024, 0) };
			foreach (var piece in PieceManager.Pieces)
			{
				mapSelection.Add(new PanelItem(new BatchObject(UITextureManager.Get("UI_map")[0], Color.White), new MPos(512, 512), piece.Name, new[] { Color.Grey + "[" + piece.Size.X + "," + piece.Size.Y + "]" },
				() =>
				{
					GameController.CreateNew(new GameStatistics(GameSaveManager.DefaultStatistic), MissionType.TEST, InteractionMode.EDITOR, custom: MapType.FromPiece(piece));
					Hide();
				}));
			}
			Content.Add(mapSelection);
			Content.Add(new Button("Back", "wooden", () => game.ShowScreen(ScreenType.MENU)) { Position = new CPos(4096, 6144, 0) });
			Content.Add(new Button("New Piece", "wooden", () => { createPieceScreen.ActiveScreen = true; }) { Position = new CPos(0, 6144, 0) } );
			Content.Add(new Button("Delete Piece", "wooden") { Position = new CPos(-4096, 6144, 0) });

			createPieceScreen = new CreatePieceScreen();
		}

		public override void Hide()
		{
			mapSelection.DisableTooltip();
		}

		public override void Render()
		{
			if (createPieceScreen.ActiveScreen)
			{
				createPieceScreen.Render();
				return;
			}

			base.Render();
		}

		public override void Tick()
		{
			if (createPieceScreen.ActiveScreen)
			{
				createPieceScreen.Tick();
				return;
			}

			base.Tick();
		}

		public override void KeyDown(Keys key, bool isControl, bool isShift, bool isAlt)
		{
			if (createPieceScreen.ActiveScreen)
			{
				createPieceScreen.KeyDown(key, isControl, isShift, isAlt);
				return;
			}
			if (key == Keys.Escape)
				game.ShowScreen(ScreenType.MENU);
		}
	}

	class CreatePieceScreen : Screen
	{
		public bool ActiveScreen = false;

		readonly TextBox sizeX;
		readonly TextBox sizeY;

		readonly TextBox name;

		public CreatePieceScreen() : base("Create Piece")
		{
			Title.Position = new CPos(0, -4096, 0);

			Content.Add(new Button("Cancel", "wooden", () => { ActiveScreen = false; }) { Position = new CPos(4096, 6144, 0) });
			Content.Add(new Button("Create", "wooden", create) { Position = new CPos(-4096, 6144, 0) });

			var size = new UITextLine(FontManager.Pixel16, TextOffset.MIDDLE) { Position = new CPos(0, -1024, 0) };
			size.SetText("Size of Piece");
			Content.Add(size);

			sizeX = new TextBox("16", "wooden", 2, true) { Position = new CPos(-1024, 0, 0) };
			Content.Add(sizeX);
			sizeY = new TextBox("16", "wooden", 2, true) { Position = new CPos(1024, 0, 0) };
			Content.Add(sizeY);
			name = new TextBox("unnamed piece", "wooden", 20, isPath: true) { Position = new CPos(0, 1536, 0) };
			Content.Add(name);

			var warning = new UITextLine(FontManager.Pixel16, TextOffset.MIDDLE)
			{
				Position = new CPos(0, 2548, 0),
				Color = Color.Red
			};
			warning.SetText("Warning: by using an name for an already existing map, you override it!");
			Content.Add(warning);
		}

		public override void KeyDown(Keys key, bool isControl, bool isShift, bool isAlt)
		{
			if (key == Keys.Escape)
				ActiveScreen = false;
		}

		void create()
		{
			if (name.Text == string.Empty)
				return;

			var size = new MPos(int.Parse(sizeX.Text), int.Parse(sizeY.Text));
			var name2 = name.Text;

			var piece = WorldCreator.CreateEmpty(name2, size);
			GameController.CreateNew(new GameStatistics(GameSaveManager.DefaultStatistic), MissionType.TEST, InteractionMode.EDITOR, custom: MapType.FromPiece(piece));
		}
	}
}
