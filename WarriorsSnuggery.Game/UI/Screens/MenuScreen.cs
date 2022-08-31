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
			Title.Position = new UIPos(0, -2048);

			var height = -1024;
			Add(new Button("Resume", "wooden", () => game.ShowScreen(ScreenType.DEFAULT, false)) { Position = new UIPos(0, height) });

			height += 1024;
			switch (game.MissionType)
			{
				case MissionType.MAIN_MENU:
					height -= 1024;
					break;
				case MissionType.TUTORIAL:
					Add(new Button("Restart", "wooden", GameController.CreateRestart) { Position = new UIPos(2048, height) });
					Add(new Button("Main Menu", "wooden", GameController.CreateMainMenu) { Position = new UIPos(-2048, height) });
					break;
				case MissionType.STORY_MENU:
				case MissionType.NORMAL_MENU:
				case MissionType.TUTORIAL_MENU:
					Add(new Button("Main Menu", "wooden", () => humanAgree(GameController.CreateMainMenu, "Are you sure to leave this game? Unsaved progress will be lost!")) { Position = new UIPos(0, height) });
					break;
				case MissionType.TEST:
					if (game.InteractionMode == InteractionMode.EDITOR)
					{
						Add(new Button("Play", "wooden", () => humanAgree(() => { GameController.CreateNew(game.Save, MissionType.TEST, custom: game.MapType); }, "Make sure you have saved the map!")) { Position = new UIPos(2048, height) });
						Add(new Button("Main Menu", "wooden", () => humanAgree(GameController.CreateMainMenu, "Are you sure to return? Unsaved progress will be lost!")) { Position = new UIPos(-2048, height) });
					}
					else
					{
						Add(new Button("Editor", "wooden", () => GameController.CreateNew(game.Save, MissionType.TEST, InteractionMode.EDITOR, game.MapType)) { Position = new UIPos(2048, height) });
						Add(new Button("Main Menu", "wooden", GameController.CreateMainMenu) { Position = new UIPos(-2048, height) });
					}
					break;
				default:
					Add(new Button("Restart", "wooden", () => humanAgree(GameController.CreateRestart, "Are you sure you want to restart? Current progress in this level will be lost!")) { Position = new UIPos(2048, height) });
					// Level is still the same as it was not finished
					Add(new Button("Headquarters", "wooden", () => humanAgree(GameController.CreateMenu, "Are you sure to return? All progress in this level will be lost!")) { Position = new UIPos(-2048, height) });
					break;
			}

			if (game.MissionType.IsCampaign() || game.MissionType == MissionType.MAIN_MENU)
			{
				height += 1024;
				Add(new Button("Load Game", "wooden", () => game.ShowScreen(ScreenType.LOADGAME)) { Position = new UIPos(game.MissionType == MissionType.MAIN_MENU ? 0 : -2048, height) });
				if (game.MissionType != MissionType.MAIN_MENU)
					Add(new Button("Save Game", "wooden", () => game.ShowScreen(ScreenType.SAVEGAME)) { Position = new UIPos(2048, height) });
			}

			height += 1024;
			Add(new Button("Settings", "wooden", () => game.ShowScreen(ScreenType.BASESETTINGS)) { Position = new UIPos(0, height) });

			if (game.MissionType == MissionType.MAIN_MENU || game.MissionType == MissionType.TEST)
			{
				height += 1024;
				Add(new Button("Editor", "wooden", () => game.ShowScreen(ScreenType.PIECESELECTION)) { Position = new UIPos(0, height) });
			}

			height += 1024;
			Add(new Button("Exit Game", "wooden", () => humanAgree(Program.Exit, "Are you sure you want to exit the game?")) { Position = new UIPos(0, height) });
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
			base.KeyDown(key, isControl, isShift, isAlt);

			if (key == Keys.Escape)
				game.ShowScreen(ScreenType.DEFAULT, false);
		}
	}
}
