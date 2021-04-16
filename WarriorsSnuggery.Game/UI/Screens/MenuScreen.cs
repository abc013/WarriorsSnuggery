using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using WarriorsSnuggery.UI.Objects;

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
			Content.Add(new Button("Resume", "wooden", () => game.ShowScreen(ScreenType.DEFAULT, false)) { Position = new CPos(0, height, 0) });

			height += 1024;
			switch (game.MissionType)
			{
				case MissionType.MAIN_MENU:
					height -= 1024;
					break;
				case MissionType.TUTORIAL:
					Content.Add(new Button("Restart", "wooden", GameController.CreateRestart) { Position = new CPos(2048, height, 0) });
					Content.Add(new Button("Main Menu", "wooden", GameController.CreateMainMenu) { Position = new CPos(-2048, height, 0) });
					break;
				case MissionType.STORY_MENU:
				case MissionType.NORMAL_MENU:
				case MissionType.TUTORIAL_MENU:
					Content.Add(new Button("Main Menu", "wooden", () => humanAgree(GameController.CreateMainMenu, "Are you sure to leave this game? Unsaved progress will be lost!")) { Position = new CPos(0, height, 0) });
					break;
				case MissionType.TEST:
					if (game.InteractionMode == InteractionMode.EDITOR)
					{
						Content.Add(new Button("Play", "wooden", () => humanAgree(() => { GameController.CreateNew(game.Statistics, MissionType.TEST, custom: game.MapType); }, "Make sure you have saved the map!")) { Position = new CPos(2048, height, 0) });
						Content.Add(new Button("Main Menu", "wooden", () => humanAgree(GameController.CreateMainMenu, "Are you sure to return? Unsaved progress will be lost!")) { Position = new CPos(-2048, height, 0) });
					}
					else
					{
						Content.Add(new Button("Editor", "wooden", () => GameController.CreateNew(game.Statistics, MissionType.TEST, InteractionMode.EDITOR, game.MapType)) { Position = new CPos(2048, height, 0) });
						Content.Add(new Button("Main Menu", "wooden", GameController.CreateMainMenu) { Position = new CPos(-2048, height, 0) });
					}
					break;
				default:
					Content.Add(new Button("Restart", "wooden", () => humanAgree(GameController.CreateRestart, "Are you sure you want to restart? Current progress in this level will be lost!")) { Position = new CPos(2048, height, 0) });
					// Level is still the same as it was not finished
					Content.Add(new Button("Headquarters", "wooden", () => humanAgree(GameController.CreateMenu, "Are you sure to return? All progress in this level will be lost!")) { Position = new CPos(-2048, height, 0) });
					break;
			}

			if (game.IsCampaign || game.MissionType == MissionType.MAIN_MENU)
			{
				height += 1024;
				Content.Add(new Button("Load Game", "wooden", () => game.ShowScreen(ScreenType.LOADGAME)) { Position = new CPos(game.MissionType == MissionType.MAIN_MENU ? 0 : -2048, height, 0) });
				if (game.MissionType != MissionType.MAIN_MENU)
					Content.Add(new Button("Save Game", "wooden", () => game.ShowScreen(ScreenType.SAVEGAME)) { Position = new CPos(2048, height, 0) });
			}

			height += 1024;
			Content.Add(new Button("Settings", "wooden", () => game.ShowScreen(ScreenType.SETTINGS)) { Position = new CPos(0, height, 0) });

			if (game.MissionType == MissionType.MAIN_MENU || game.MissionType == MissionType.TEST)
			{
				height += 1024;
				Content.Add(new Button("Editor", "wooden", () => game.ShowScreen(ScreenType.PIECESELECTION)) { Position = new CPos(0, height, 0) });
			}

			height += 1024;
			Content.Add(new Button("Exit Game", "wooden", () => humanAgree(Program.Exit, "Are you sure you want to exit the game?")) { Position = new CPos(0, height, 0) });
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
