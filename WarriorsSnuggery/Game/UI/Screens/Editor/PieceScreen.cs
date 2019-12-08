using System.IO;
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
				mapSelection.Add(new PanelItem(CPos.Zero, new ImageRenderable(TextureManager.Texture("UI_map")), new MPos(512, 512), piece.Name, new[] { Color.Grey + "[" + piece.Size.X + "," + piece.Size.Y + "]" },
				() =>
				{
					GameController.CreateNew(new GameStatistics(GameSaveManager.DefaultStatistic), GameType.EDITOR, custom: MapInfo.EditorMapTypeFromPiece(piece.InnerName, piece.Size));
					Hide();
				}));
			}
			Content.Add(ButtonCreator.Create("wooden", new CPos(4096, 6144, 0), "Back", () => game.ChangeScreen(ScreenType.MENU)));
			Content.Add(ButtonCreator.Create("wooden", new CPos(0, 6144, 0), "New Piece", () => { createPieceScreen.ActiveScreen = true; }));
			Content.Add(ButtonCreator.Create("wooden", new CPos(-4096, 6144, 0), "Delete Piece", () => { }));

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

			mapSelection.Render();
		}

		public override void Tick()
		{
			if (createPieceScreen.ActiveScreen)
			{
				createPieceScreen.Tick();
				return;
			}

			base.Tick();

			mapSelection.Tick();

			if (KeyInput.IsKeyDown("escape", 10))
				game.ChangeScreen(ScreenType.MENU);
		}

		public override void Dispose()
		{
			base.Dispose();

			mapSelection.Dispose();

			createPieceScreen.Dispose();
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

			Content.Add(ButtonCreator.Create("wooden", new CPos(4096, 6144, 0), "Cancel", () => { ActiveScreen = false; }));
			Content.Add(ButtonCreator.Create("wooden", new CPos(-4096, 6144, 0), "Create", () => { create(); }));

			var size = new TextLine(new CPos(0, -1024, 0), IFont.Pixel16, TextLine.OffsetType.MIDDLE);
			size.SetText("Size of Piece");
			Content.Add(size);
			sizeX = TextBoxCreator.Create("wooden", new CPos(1024, 0, 0), "16", 2, true);
			sizeY = TextBoxCreator.Create("wooden", new CPos(-1024, 0, 0), "16", 2, true);
			name = TextBoxCreator.Create("wooden", new CPos(0, 1536, 0), "unnamed piece", 20, false);
			var warning = new TextLine(new CPos(0, 2548, 0), IFont.Pixel16, TextLine.OffsetType.MIDDLE);
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

		public override void Dispose()
		{
			base.Dispose();

			sizeX.Dispose();
			sizeY.Dispose();
			name.Dispose();
		}

		void create()
		{
			var size = new MPos(int.Parse(sizeX.Text), int.Parse(sizeY.Text));
			var path = FileExplorer.Maps + @"\maps";

			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);

			FileExplorer.CreateFile(path + "\\", name.Text, ".yaml");
			using (var stream = new StreamWriter(path + "\\map.yaml"))
			{
				stream.WriteLine("Name=" + name.Text);
				stream.WriteLine("Size=" + size.X + "," + size.Y);
				var terrain = "0";
				for (int i = 1; i < size.X * size.Y; i++)
					terrain += ",0";
				stream.WriteLine("Terrain=" + terrain);
				var walls = "-1";
				for (int i = 1; i < size.X * size.Y * 2; i++)
					walls += ",-1";
				stream.WriteLine("Walls=" + walls);
			}
			// Load piece into cache
			PieceManager.RefreshPiece(name.Text);

			GameController.CreateNew(new GameStatistics(GameSaveManager.DefaultStatistic), GameType.EDITOR, custom: MapInfo.EditorMapTypeFromPiece(name.Text, size));
		}
	}
}
