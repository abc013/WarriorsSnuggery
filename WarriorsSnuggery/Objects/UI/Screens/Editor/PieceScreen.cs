using System.IO;
using System.Linq;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Maps;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI
{
	public class PieceScreen : Screen
	{
		readonly Game game;

		readonly PanelList mapSelection;

		readonly CreatePieceScreen createPieceScreen;

		public PieceScreen(Game game) : base("Piece Selection")
		{
			this.game = game;
			Title.Position = new CPos(0, -4096, 0);

			mapSelection = new PanelList(new CPos(0, 1024, 0), new MPos(4096, 4096), new MPos(512, 512), PanelManager.Get("wooden"));
			foreach (var piece in PieceManager.GetPieces())
			{
				mapSelection.Add(new PanelItem(CPos.Zero, new BatchObject(UITextureManager.Get("UI_map")[0], Color.White), new MPos(512, 512), piece.Name, new[] { Color.Grey + "[" + piece.Size.X + "," + piece.Size.Y + "]" },
				() =>
				{
					GameController.CreateNew(new GameStatistics(GameSaveManager.DefaultStatistic), GameType.EDITOR, custom: MapInfo.EditorMapTypeFromPiece(piece.InnerName, piece.Size));
					Hide();
				}));
			}
			Content.Add(mapSelection);
			Content.Add(new Button(new CPos(4096, 6144, 0), "Back", "wooden", () => game.ChangeScreen(ScreenType.MENU)));
			Content.Add(new Button(new CPos(0, 6144, 0), "New Piece", "wooden", () => { createPieceScreen.ActiveScreen = true; }));
			Content.Add(new Button(new CPos(-4096, 6144, 0), "Delete Piece", "wooden", () => { }));

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

			if (KeyInput.IsKeyDown("escape", 10))
				game.ChangeScreen(ScreenType.MENU);
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

			Content.Add(new Button(new CPos(4096, 6144, 0), "Cancel", "wooden", () => { ActiveScreen = false; }));
			Content.Add(new Button(new CPos(-4096, 6144, 0), "Create", "wooden", () => { create(); }));

			var size = new TextLine(new CPos(0, -1024, 0), FontManager.Pixel16, TextLine.OffsetType.MIDDLE);
			size.SetText("Size of Piece");
			Content.Add(size);
			sizeX = new TextBox(new CPos(1024, 0, 0), "16", "wooden", 2, true);
			sizeY = new TextBox(new CPos(-1024, 0, 0), "16", "wooden", 2, true);
			name = new TextBox(new CPos(0, 1536, 0), "unnamed piece", "wooden", 20, isPath: true);
			var warning = new TextLine(new CPos(0, 2548, 0), FontManager.Pixel16, TextLine.OffsetType.MIDDLE);
			warning.SetColor(Color.Red);
			warning.SetText("Warning: by using an name for an already existing map, you override it!");
			Content.Add(warning);
		}

		public override void Render()
		{
			base.Render();

			sizeX.Render();
			sizeY.Render();
			name.Render();
		}

		public override void Tick()
		{
			base.Tick();

			if (KeyInput.IsKeyDown("escape", 10))
				ActiveScreen = false;

			sizeX.Tick();
			sizeY.Tick();
			name.Tick();
		}

		void create()
		{
			if (name.Text == string.Empty)
				return;

			var size = new MPos(int.Parse(sizeX.Text), int.Parse(sizeY.Text));
			var path = FileExplorer.Maps + @"\maps";

			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);

			using (var stream = new StreamWriter(FileExplorer.CreateFile(path + "\\", name.Text, ".yaml")))
			{
				stream.WriteLine("Name=" + name.Text);
				stream.WriteLine("Size=" + size.X + "," + size.Y);
				var terrain = string.Join(",", Enumerable.Repeat("0", size.X * size.Y));
				stream.WriteLine("Terrain=" + terrain);
				var walls = string.Join(",", Enumerable.Repeat("-1", (size.X + 1) * (size.Y + 1) * 2 * 2));
				stream.WriteLine("Walls=" + walls);
			}
			// Load piece into cache
			PieceManager.RefreshPiece(name.Text);

			GameController.CreateNew(new GameStatistics(GameSaveManager.DefaultStatistic), GameType.EDITOR, custom: MapInfo.EditorMapTypeFromPiece(name.Text, size));
		}
	}
}
