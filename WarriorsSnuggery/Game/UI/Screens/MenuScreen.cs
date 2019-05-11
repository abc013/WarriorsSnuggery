/*
 * User: Andreas
 * Date: 01.07.2018
 * Time: 01:04
 */
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI
{
	public class MenuScreen : Screen
	{
		readonly Game game;

		readonly Button resume;
		readonly Button restart;
		readonly Button menu;
		readonly ColoredLine cut1;
		readonly Button save;
		readonly Button load;
		readonly ColoredLine cut2;
		readonly Button settings;
		readonly Button editor;
		readonly ColoredLine cut3;
		readonly Button leave;

		public bool Visible { get; private set;}

		public MenuScreen(Game game) : base("Menu")
		{
			this.game = game;
			Title.Position = new CPos(0,-2048, 0);

			var height = -1024;
			resume = ButtonCreator.Create("wooden", new CPos(0, height, 0), "Resume", () => game.Pause(false));

			height += 1024;
			switch(game.Type)
			{
				case GameType.EDITOR:
					restart = ButtonCreator.Create("wooden", new CPos(2048, height, 0), "Play", () => Window.Current.NewGame(game.Statistics, GameType.NORMAL, true, Maps.MapType.ConvertGameType(game.MapType, GameType.TEST)));
					menu = ButtonCreator.Create("wooden", new CPos(-2048, height, 0), "Main Menu", () => Window.Current.NewGame(game.Statistics, GameType.MAINMENU));
					break;
				case GameType.MAINMENU:
					height -= 1024;
					break;
				case GameType.TUTORIAL:
					restart = ButtonCreator.Create("wooden", new CPos(2048, height, 0), "Restart", () => Window.Current.NewGame(game.Statistics, GameType.TUTORIAL, true));
					menu = ButtonCreator.Create("wooden", new CPos(-2048, height, 0), "Main Menu", () => Window.Current.NewGame(game.Statistics, GameType.MAINMENU));
					break;
				case GameType.MENU:
					menu = ButtonCreator.Create("wooden", new CPos(0, height, 0), "Main Menu", () => Window.Current.NewGame(game.Statistics, GameType.MAINMENU));
					break;
				case GameType.TEST:
					restart = ButtonCreator.Create("wooden", new CPos(2048, height, 0), "Editor", () => Window.Current.NewGame(game.Statistics, GameType.EDITOR, true, Maps.MapType.ConvertGameType(game.MapType, GameType.EDITOR)));
					menu = ButtonCreator.Create("wooden", new CPos(-2048, height, 0), "Main Menu", () => Window.Current.NewGame(game.Statistics, GameType.MAINMENU));
					break;
				default:
					restart = ButtonCreator.Create("wooden", new CPos(2048, height, 0), "Restart", () => Window.Current.NewGame(game.Statistics, GameType.NORMAL, true));
					menu = ButtonCreator.Create("wooden", new CPos(-2048, height, 0), "Headquarters", () => { game.Statistics.Level--; Window.Current.NewGame(game.Statistics, GameType.MENU); }); // Level is still the same as it was not finished
					break;
			}

			height += 512;
			cut1 = new ColoredLine(new CPos(4096, height, 0), Color.White, 8f)
			{
				Rotation = new CPos(0, 0, 90)
			};
			if (game.Type != GameType.EDITOR && game.Type != GameType.TUTORIAL && game.Type != GameType.TEST)
			{
				height += 512;
				load = ButtonCreator.Create("wooden", new CPos(game.Type == GameType.MAINMENU ? 0 : -2048, height, 0), "Load Game", () => game.ChangeScreen(ScreenType.LOAD));
				if (game.Type != GameType.MAINMENU)
					save = ButtonCreator.Create("wooden", new CPos(2048, height, 0), "Save Game", () => game.ChangeScreen(ScreenType.SAVE));

				height += 512;
				cut2 = new ColoredLine(new CPos(4096, height, 0), Color.White, 8f)
				{
					Rotation = new CPos(0, 0, 90)
				};
			}
			height += 512;
			settings = ButtonCreator.Create("wooden", new CPos(0, height, 0), "Settings", () => game.ChangeScreen(ScreenType.SETTINGS));

			if (game.Type == GameType.MAINMENU || game.Type == GameType.EDITOR)
			{
				height += 1024;
				editor = ButtonCreator.Create("wooden", new CPos(0, height, 0), "Editor", () => game.ChangeScreen(ScreenType.EDITORSELECTION));
			}

			height += 512;
			cut3 = new ColoredLine(new CPos(4096, height, 0), Color.White, 8f)
			{
				Rotation = new CPos(0, 0, 90)
			};
			height += 512;
			leave = ButtonCreator.Create("wooden", new CPos(0, height, 0), "Exit Game", Window.Current.Exit);
		}

		public override void Render()
		{
			base.Render();
			
			resume.Render();

			if (restart != null)
				restart.Render();

			if (menu != null)
				menu.Render();

			cut1.Render();
			if (game.Type != GameType.EDITOR && game.Type != GameType.TUTORIAL && game.Type != GameType.TEST)
			{
				load.Render();
				if (game.Type != GameType.MAINMENU)
					save.Render();
				cut2.Render();
			}
			settings.Render();

			if (game.Type == GameType.MAINMENU || game.Type == GameType.EDITOR)
				editor.Render();

			cut3.Render();
			leave.Render();
		}

		public override void Tick()
		{
			base.Tick();

			resume.Tick();

			if (restart != null)
				restart.Tick();

			if (menu != null)
				menu.Tick();

			if (game.Type != GameType.EDITOR && game.Type != GameType.TUTORIAL && game.Type != GameType.TEST)
			{
				load.Tick();
				if (game.Type != GameType.MAINMENU)
					save.Tick();
			}
			settings.Tick();

			if (game.Type == GameType.MAINMENU || game.Type == GameType.EDITOR || game.Type == GameType.TEST)
				editor.Tick();

			leave.Tick();

			if (KeyInput.IsKeyDown("escape", 10))
			{
				game.Pause(false);
				game.ChangeScreen(ScreenType.DEFAULT);
			}
		}

		public override void Dispose()
		{
			base.Dispose();

			resume.Dispose();

			if (restart != null)
				restart.Dispose();

			if (menu != null)
				menu.Dispose();

			cut1.Dispose();

			if (game.Type != GameType.EDITOR && game.Type != GameType.TUTORIAL && game.Type != GameType.TEST)
			{
				load.Dispose();
				if (game.Type != GameType.MAINMENU)
					save.Dispose();
				cut2.Dispose();
			}
			settings.Dispose();

			if (game.Type == GameType.MAINMENU || game.Type == GameType.EDITOR)
				editor.Dispose();

			cut3.Dispose();
			leave.Dispose();
		}
	}
}
