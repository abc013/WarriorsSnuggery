using OpenTK.Windowing.GraphicsLibraryFramework;
using System;

namespace WarriorsSnuggery.UI.Screens
{
	public class MenuScreen : Screen
	{
		readonly Game game;

		public bool Visible { get; private set; }

		public MenuScreen(Game game) : base("Menu")
		{
			this.game = game;
			Title.Position = new CPos(0, -2048, 0);

			var height = -1024;
			Content.Add(new Button(new CPos(0, height, 0), "Resume", "wooden", () => game.ShowScreen(ScreenType.DEFAULT, false)));

			height += 1024;
			switch (game.Type)
			{
				case GameType.EDITOR:
					Content.Add(new Button(new CPos(2048, height, 0), "Play", "wooden", () => humanAgree(() => { GameController.CreateNew(game.Statistics, GameType.NORMAL, Maps.MapInfo.ConvertGameType(game.MapType, GameType.TEST)); }, "Make sure you have saved the map!")));
					Content.Add(new Button(new CPos(-2048, height, 0), "Main Menu", "wooden", () => humanAgree(() => { GameController.CreateReturn(GameType.MAINMENU); }, "Are you sure to return? Unsaved progress will be lost!")));
					break;
				case GameType.MAINMENU:
					height -= 1024;
					break;
				case GameType.TUTORIAL:
					Content.Add(new Button(new CPos(2048, height, 0), "Restart", "wooden", () => GameController.CreateRestart()));
					Content.Add(new Button(new CPos(-2048, height, 0), "Main Menu", "wooden", () => GameController.CreateReturn(GameType.MAINMENU)));
					break;
				case GameType.MENU:
					Content.Add(new Button(new CPos(0, height, 0), "Main Menu", "wooden", () => humanAgree(() => { GameController.CreateReturn(GameType.MAINMENU); }, "Are you sure to leave this game? Unsaved progress will be lost!")));
					break;
				case GameType.TEST:
					Content.Add(new Button(new CPos(2048, height, 0), "Editor", "wooden", () => GameController.CreateNew(game.Statistics, GameType.EDITOR, Maps.MapInfo.ConvertGameType(game.MapType, GameType.EDITOR))));
					Content.Add(new Button(new CPos(-2048, height, 0), "Main Menu", "wooden", () => GameController.CreateReturn(GameType.MAINMENU)));
					break;
				default:
					Content.Add(new Button(new CPos(2048, height, 0), "Restart", "wooden", () => humanAgree(() => { GameController.CreateRestart(); }, "Are you sure you want to restart? Current progress in this level will be lost!")));
					Content.Add(new Button(new CPos(-2048, height, 0), "Headquarters", "wooden", () => humanAgree(() => { GameController.CreateReturn(GameType.MENU); }, "Are you sure to return? All progress in this level will be lost!"))); // Level is still the same as it was not finished
					break;
			}

			if (game.Type != GameType.EDITOR && game.Type != GameType.TUTORIAL && game.Type != GameType.TEST)
			{
				height += 1024;
				Content.Add(new Button(new CPos(game.Type == GameType.MAINMENU ? 0 : -2048, height, 0), "Load Game", "wooden", () => game.ShowScreen(ScreenType.LOADGAME)));
				if (game.Type != GameType.MAINMENU)
					Content.Add(new Button(new CPos(2048, height, 0), "Save Game", "wooden", () => game.ShowScreen(ScreenType.SAVEGAME)));
			}

			height += 1024;
			Content.Add(new Button(new CPos(0, height, 0), "Settings", "wooden", () => game.ShowScreen(ScreenType.SETTINGS)));

			if (game.Type == GameType.MAINMENU || game.Type == GameType.EDITOR)
			{
				height += 1024;
				Content.Add(new Button(new CPos(0, height, 0), "Editor", "wooden", () => game.ShowScreen(ScreenType.PIECESELECTION)));
			}

			height += 1024;
			Content.Add(new Button(new CPos(0, height, 0), "Exit Game", "wooden", () => humanAgree(Program.Exit, "Are you sure you want to exit the game?")));
		}

		void humanAgree(Action onAgree, string text)
		{
			void onDecline()
			{
				game.ShowScreen(ScreenType.MENU);
			}
			game.ShowDecisionScreen(onDecline, onAgree, text);
		}

		public override void KeyDown(Keys key, bool isControl, bool isShift, bool isAlt)
		{
			if (key == Keys.Escape)
				game.ShowScreen(ScreenType.DEFAULT, false);
		}
	}
}
