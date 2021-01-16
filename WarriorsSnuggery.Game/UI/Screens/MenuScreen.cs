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
			switch (game.MissionType)
			{
				case MissionType.MAIN_MENU:
					height -= 1024;
					break;
				case MissionType.TUTORIAL:
					Content.Add(new Button(new CPos(2048, height, 0), "Restart", "wooden", GameController.CreateRestart));
					Content.Add(new Button(new CPos(-2048, height, 0), "Main Menu", "wooden", GameController.CreateMainMenu));
					break;
				case MissionType.STORY_MENU:
				case MissionType.NORMAL_MENU:
				case MissionType.TUTORIAL_MENU:
					Content.Add(new Button(new CPos(0, height, 0), "Main Menu", "wooden", () => humanAgree(GameController.CreateMainMenu, "Are you sure to leave this game? Unsaved progress will be lost!")));
					break;
				case MissionType.TEST:
					if (game.InteractionMode == InteractionMode.EDITOR)
					{
						Content.Add(new Button(new CPos(2048, height, 0), "Play", "wooden", () => humanAgree(() => { GameController.CreateNew(game.Statistics, MissionType.TEST, custom: game.MapType); }, "Make sure you have saved the map!")));
						Content.Add(new Button(new CPos(-2048, height, 0), "Main Menu", "wooden", () => humanAgree(GameController.CreateMainMenu, "Are you sure to return? Unsaved progress will be lost!")));
					}
					else
					{
						Content.Add(new Button(new CPos(2048, height, 0), "Editor", "wooden", () => GameController.CreateNew(game.Statistics, MissionType.TEST, InteractionMode.EDITOR, game.MapType)));
						Content.Add(new Button(new CPos(-2048, height, 0), "Main Menu", "wooden", GameController.CreateMainMenu));
					}
					break;
				default:
					Content.Add(new Button(new CPos(2048, height, 0), "Restart", "wooden", () => humanAgree(GameController.CreateRestart, "Are you sure you want to restart? Current progress in this level will be lost!")));
					Content.Add(new Button(new CPos(-2048, height, 0), "Headquarters", "wooden", () => humanAgree(GameController.CreateMenu, "Are you sure to return? All progress in this level will be lost!"))); // Level is still the same as it was not finished
					break;
			}

			if (game.IsCampaign || game.MissionType == MissionType.MAIN_MENU)
			{
				height += 1024;
				Content.Add(new Button(new CPos(game.MissionType == MissionType.MAIN_MENU ? 0 : -2048, height, 0), "Load Game", "wooden", () => game.ShowScreen(ScreenType.LOADGAME)));
				if (game.MissionType != MissionType.MAIN_MENU)
					Content.Add(new Button(new CPos(2048, height, 0), "Save Game", "wooden", () => game.ShowScreen(ScreenType.SAVEGAME)));
			}

			height += 1024;
			Content.Add(new Button(new CPos(0, height, 0), "Settings", "wooden", () => game.ShowScreen(ScreenType.SETTINGS)));

			if (game.MissionType == MissionType.MAIN_MENU || game.MissionType == MissionType.TEST)
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
