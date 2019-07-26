/*
 * User: Andreas
 * Date: 27.10.2018
 * Time: 15:44
 */
using System;
using System.Collections.Generic;
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

		readonly Button back;
		readonly Button @new;
		readonly Button delete;

		readonly CreatePieceScreen createPieceScreen;

		public PieceScreen(Game game) : base("Piece Selection")
		{
			this.game = game;
			Title.Position = new CPos(0, -4096, 0);

			mapSelection = new PanelList(new CPos(0, 1024, 0), new MPos(4096, 4096), new MPos(512, 512), 4, "UI_wood1", "UI_wood3", "UI_wood2");
			foreach (var dir in Directory.GetDirectories(FileExplorer.Maps))
			{
				var file = Directory.GetFiles(dir).Where(s => s.EndsWith(".yaml", StringComparison.CurrentCulture));
				foreach (var map in file)
				{
					var parts = new Dictionary<string, MPos>();
					var name = map.Remove(0, map.LastIndexOf('\\') + 1);
					name = name.Remove(name.Length - 5);
					parts.Add(name, MPos.Zero);

					// TODO: read all maps in in the begin? BUT: would need a big amount of memory 
					var size = RuleReader.Read(FileExplorer.FindPath(FileExplorer.Maps, name, ".yaml"), name + ".yaml").First(n => n.Key == "Size").Convert<MPos>();

					mapSelection.Add(new PanelItem(CPos.Zero, new ImageRenderable(TextureManager.Texture("UI_map")), new MPos(512, 512), name, new[] { Color.Grey + "[" + size.X + "," + size.Y + "]" },
						() =>
						{
							Window.Current.NewGame(new GameStatistics(GameSaveManager.DefaultStatistic), GameType.EDITOR, custom: MapType.EditorMapTypeFromPiece(name, size));
							Hide();
						}));
				}
			}
			back = ButtonCreator.Create("wooden", new CPos(4096, 6144, 0), "Back", () => game.ChangeScreen(ScreenType.MENU));
			@new = ButtonCreator.Create("wooden", new CPos(0, 6144, 0), "New Piece", () => { createPieceScreen.ActiveScreen = true; });
			delete = ButtonCreator.Create("wooden", new CPos(-4096, 6144, 0), "Delete Piece", () => { });

			createPieceScreen = new CreatePieceScreen(this, game);
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

			back.Render();
			@new.Render();
			delete.Render();
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
			{
				game.ChangeScreen(ScreenType.MENU);
			}

			back.Tick();
			@new.Tick();
			delete.Tick();
		}

		public override void Dispose()
		{
			base.Dispose();

			mapSelection.Dispose();

			back.Dispose();
			@new.Dispose();
			delete.Dispose();

			createPieceScreen.Dispose();
		}
	}

	class CreatePieceScreen : Screen
	{
		public bool ActiveScreen = false;

		readonly Game game;
		readonly PieceScreen screen;

		readonly Button cancel;
		readonly Button okay;

		readonly TextLine size;
		readonly TextBox sizeX;
		readonly TextBox sizeY;

		readonly TextLine warning;
		readonly TextBox name;

		public CreatePieceScreen(PieceScreen screen, Game game) : base("Create Piece")
		{
			this.screen = screen;
			this.game = game;
			Title.Position = new CPos(0, -4096, 0);

			cancel = ButtonCreator.Create("wooden", new CPos(4096, 6144, 0), "Cancel", () => { ActiveScreen = false; });
			okay = ButtonCreator.Create("wooden", new CPos(-4096, 6144, 0), "Create", () => { Create(); });

			size = new TextLine(new CPos(0, -1024, 0), IFont.Pixel16, TextLine.OffsetType.MIDDLE);
			size.SetText("Size of Piece");
			sizeX = TextBoxCreator.Create("wooden", new CPos(1024, 0, 0), "16", 2, true);
			sizeY = TextBoxCreator.Create("wooden", new CPos(-1024, 0, 0), "16", 2, true);
			name = TextBoxCreator.Create("wooden", new CPos(0, 1536, 0), "unnamed piece", 20, false);
			warning = new TextLine(new CPos(0, 2548, 0), IFont.Pixel16, TextLine.OffsetType.MIDDLE);
			warning.SetColor(Color.Red);
			warning.SetText("Warning: by using an name for an already existing map, you override it!");
		}

		public override void Render()
		{
			base.Render();

			cancel.Render();
			okay.Render();

			size.Render();
			sizeX.Render();
			sizeY.Render();
			name.Render();
			warning.Render();
		}

		public override void Tick()
		{
			base.Tick();

			if (KeyInput.IsKeyDown("escape", 10))
			{
				ActiveScreen = false;
			}

			cancel.Tick();
			okay.Tick();

			size.Tick();
			sizeX.Tick();
			sizeY.Tick();
			name.Tick();
			warning.Tick();
		}

		public override void Dispose()
		{
			base.Dispose();

			cancel.Dispose();
			okay.Dispose();

			size.Dispose();
			sizeX.Dispose();
			sizeY.Dispose();
			name.Dispose();
			warning.Dispose();
		}

		void Create()
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

			Window.Current.NewGame(new GameStatistics(GameSaveManager.DefaultStatistic), GameType.EDITOR, custom: MapType.EditorMapTypeFromPiece(name.Text, size));
		}
	}
}
