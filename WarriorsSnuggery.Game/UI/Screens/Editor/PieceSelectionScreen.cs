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
				mapSelection.Add(new PanelListItem(new BatchObject(UISpriteManager.Get("UI_map")[0]), new MPos(512, 512), piece.Name, new[] { Color.Grey + "[" + piece.Size.X + "," + piece.Size.Y + "]" },
				() =>
				{
					GameController.CreateNew(GameSaveManager.DefaultSave.Copy(), MissionType.TEST, InteractionMode.EDITOR, custom: MapType.FromPiece(piece));
					Hide();
				}));
			}
			Add(mapSelection);
			Add(new Button("Back", "wooden", () => game.ShowScreen(ScreenType.MENU)) { Position = new CPos(4096, 6144, 0) });
			Add(new Button("New Piece", "wooden", () => { createPieceScreen.ActiveScreen = true; }) { Position = new CPos(0, 6144, 0) } );
			Add(new Button("Delete Piece", "wooden") { Position = new CPos(-4096, 6144, 0) });

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

			base.KeyDown(key, isControl, isShift, isAlt);

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

			Add(new Button("Cancel", "wooden", () => { ActiveScreen = false; }) { Position = new CPos(4096, 6144, 0) });
			Add(new Button("Create", "wooden", create) { Position = new CPos(-4096, 6144, 0) });

			var size = new UITextLine(FontManager.Default, TextOffset.MIDDLE) { Position = new CPos(0, -1024, 0) };
			size.SetText("Size of Piece");
			Add(size);

			sizeX = new TextBox("wooden", 2, InputType.NUMBERS)
			{
				Position = new CPos(-1024, 0, 0),
				Text = "16"
			};
			Add(sizeX);

			sizeY = new TextBox("wooden", 2, InputType.NUMBERS)
			{
				Position = new CPos(1024, 0, 0),
				Text = "16"
			};
			Add(sizeY);
			name = new TextBox("wooden", 20, InputType.PATH)
			{
				Position = new CPos(0, 1536, 0),
				Text = "unnamed piece"
			};
			Add(name);

			var warning = new UITextLine(FontManager.Default, TextOffset.MIDDLE)
			{
				Position = new CPos(0, 2548, 0),
				Color = Color.Red
			};
			warning.SetText("Warning: by using an name for an already existing map, you override it!");
			Add(warning);
		}

		public override void KeyDown(Keys key, bool isControl, bool isShift, bool isAlt)
		{
			base.KeyDown(key, isControl, isShift, isAlt);

			if (key == Keys.Escape)
				ActiveScreen = false;
		}

		void create()
		{
			if (name.Text == string.Empty)
				return;

			var size = new MPos(int.Parse(sizeX.Text), int.Parse(sizeY.Text));
			var name2 = name.Text;

			var piece = PieceCreator.CreateEmpty(name2, size);
			GameController.CreateNew(GameSaveManager.DefaultSave.Copy(), MissionType.TEST, InteractionMode.EDITOR, custom: MapType.FromPiece(piece));
		}
	}
}
