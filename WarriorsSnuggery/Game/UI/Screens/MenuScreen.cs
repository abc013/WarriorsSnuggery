using System;

namespace WarriorsSnuggery.UI
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
			Content.Add(ButtonCreator.Create("wooden", new CPos(0, height, 0), "Resume", () => { game.Pause(false); game.ScreenControl.ShowScreen(ScreenType.DEFAULT); }));

			height += 1024;
			switch (game.Type)
			{
				case GameType.EDITOR:
					Content.Add(ButtonCreator.Create("wooden", new CPos(2048, height, 0), "Play", () => humanAgree(() => { GameController.CreateNew(game.Statistics, GameType.NORMAL, Maps.MapInfo.ConvertGameType(game.MapType, GameType.TEST)); }, "Make sure you have saved the map!")));
					Content.Add(ButtonCreator.Create("wooden", new CPos(-2048, height, 0), "Main Menu", () => humanAgree(() => { GameController.CreateReturn(GameType.MAINMENU); }, "Are you sure to return? Unsaved progress will be lost!")));
					break;
				case GameType.MAINMENU:
					height -= 1024;
					break;
				case GameType.TUTORIAL:
					Content.Add(ButtonCreator.Create("wooden", new CPos(2048, height, 0), "Restart", () => GameController.CreateRestart()));
					Content.Add(ButtonCreator.Create("wooden", new CPos(-2048, height, 0), "Main Menu", () => GameController.CreateReturn(GameType.MAINMENU)));
					break;
				case GameType.MENU:
					Content.Add(ButtonCreator.Create("wooden", new CPos(0, height, 0), "Main Menu", () => humanAgree(() => { GameController.CreateReturn(GameType.MAINMENU); }, "Are you sure to leave this game? Unsaved progress will be lost!")));
					break;
				case GameType.TEST:
					Content.Add(ButtonCreator.Create("wooden", new CPos(2048, height, 0), "Editor", () => GameController.CreateNew(game.Statistics, GameType.EDITOR, Maps.MapInfo.ConvertGameType(game.MapType, GameType.EDITOR))));
					Content.Add(ButtonCreator.Create("wooden", new CPos(-2048, height, 0), "Main Menu", () => GameController.CreateReturn(GameType.MAINMENU)));
					break;
				default:
					Content.Add(ButtonCreator.Create("wooden", new CPos(2048, height, 0), "Restart", () => humanAgree(() => { GameController.CreateRestart(); }, "Are you sure you want to restart? Current progress in this level will be lost!")));
					Content.Add(ButtonCreator.Create("wooden", new CPos(-2048, height, 0), "Headquarters", () => humanAgree(() => { GameController.CreateReturn(GameType.MENU); }, "Are you sure to return? All progress in this level will be lost!"))); // Level is still the same as it was not finished
					break;
			}

			if (game.Type != GameType.EDITOR && game.Type != GameType.TUTORIAL && game.Type != GameType.TEST)
			{
				height += 1024;
				Content.Add(ButtonCreator.Create("wooden", new CPos(game.Type == GameType.MAINMENU ? 0 : -2048, height, 0), "Load Game", () => game.ChangeScreen(ScreenType.LOAD)));
				if (game.Type != GameType.MAINMENU)
					Content.Add(ButtonCreator.Create("wooden", new CPos(2048, height, 0), "Save Game", () => game.ChangeScreen(ScreenType.SAVE)));
			}

			height += 1024;
			Content.Add(ButtonCreator.Create("wooden", new CPos(0, height, 0), "Settings", () => game.ChangeScreen(ScreenType.SETTINGS)));

			if (game.Type == GameType.MAINMENU || game.Type == GameType.EDITOR)
			{
				height += 1024;
				Content.Add(ButtonCreator.Create("wooden", new CPos(0, height, 0), "Editor", () => game.ChangeScreen(ScreenType.EDITORSELECTION)));
			}

			height += 1024;
			Content.Add(ButtonCreator.Create("wooden", new CPos(0, height, 0), "Exit Game", () => humanAgree(Program.Exit, "Are you sure you want to exit the game?")));
		}

		void humanAgree(Action onAgree, string text)
		{
			void onDecline()
			{
				game.ScreenControl.ShowScreen(ScreenType.MENU);
			}
			game.ScreenControl.SetDecision(onDecline, onAgree, text);
			game.ScreenControl.ShowScreen(ScreenType.DECISION);
		}

		public override void Tick()
		{
			base.Tick();

			if (KeyInput.IsKeyDown("escape", 10))
			{
				game.Pause(false);
				game.ChangeScreen(ScreenType.DEFAULT);
			}
		}
	}
}
