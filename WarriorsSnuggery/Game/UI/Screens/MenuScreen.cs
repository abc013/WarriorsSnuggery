/*
 * User: Andreas
 * Date: 01.07.2018
 * Time: 01:04
 */
using System;
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
					restart = ButtonCreator.Create("wooden", new CPos(2048, height, 0), "Play Piece", () => Window.Current.NewGame(game.Stats, GameType.NORMAL, true, Maps.MapType.ConvertGameType(game.MapType, GameType.NORMAL)));
					menu = ButtonCreator.Create("wooden", new CPos(-2048, height, 0), "Main Menu", () => Window.Current.NewGame(game.Stats, GameType.MAINMENU));
					break;
				case GameType.MAINMENU:
					restart = ButtonCreator.Create("wooden", new CPos(2048, height, 0), "I Do Nothing", () => { });
					menu = ButtonCreator.Create("wooden", new CPos(-2048, height, 0), "I Do Nothing", () => { });
					break;
				case GameType.TUTORIAL:
					restart = ButtonCreator.Create("wooden", new CPos(2048, height, 0), "Restart Tutorial", () => Window.Current.NewGame(game.Stats, GameType.TUTORIAL, true));
					menu = ButtonCreator.Create("wooden", new CPos(-2048, height, 0), "Main Menu", () => Window.Current.NewGame(game.Stats, GameType.MAINMENU));
					break;
				case GameType.MENU:
					restart = ButtonCreator.Create("wooden", new CPos(2048, height, 0), "I Do Nothing", () => { });
					menu = ButtonCreator.Create("wooden", new CPos(-2048, height, 0), "Main Menu", () => Window.Current.NewGame(game.Stats, GameType.MAINMENU));
					break;
				default:
					restart = ButtonCreator.Create("wooden", new CPos(2048, height, 0), "Restart Level", () => Window.Current.NewGame(game.Stats, GameType.NORMAL, true));
					menu = ButtonCreator.Create("wooden", new CPos(-2048, height, 0), "Evacuate", () => Window.Current.NewGame(game.Stats, GameType.MENU));
					break;
			}

			height += 512;
			cut1 = new ColoredLine(new CPos(4096, height, 0), Color.White, 8f)
			{
				Rotation = new CPos(0, 0, 90)
			};
			if (game.Type != GameType.EDITOR)
			{
				height += 512;
				load = ButtonCreator.Create("wooden", new CPos(-2048, height, 0), "Load Game", () => game.ChangeScreen(ScreenType.LOAD));
				save = ButtonCreator.Create("wooden", new CPos(2048, height, 0), "Save Game", () => game.ChangeScreen(ScreenType.SAVE));

				height += 512;
				cut2 = new ColoredLine(new CPos(4096, height, 0), Color.White, 8f)
				{
					Rotation = new CPos(0, 0, 90)
				};
			}
			height += 512;
			settings = ButtonCreator.Create("wooden", new CPos(0, height, 0), "Settings", () => game.ChangeScreen(ScreenType.SETTINGS));

			height += 1024;
			editor = ButtonCreator.Create("wooden", new CPos(0, height, 0), "Editor", () => game.ChangeScreen(ScreenType.EDITORSELECTION));

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
			restart.Render();
			menu.Render();
			cut1.Render();
			if (game.Type != GameType.EDITOR)
			{
				load.Render();
				save.Render();
				cut2.Render();
			}
			settings.Render();
			editor.Render();
			cut3.Render();
			leave.Render();
		}

		public override void Tick()
		{
			base.Tick();

			resume.Tick();
			restart.Tick();
			menu.Tick();
			if (game.Type != GameType.EDITOR)
			{
				load.Tick();
				save.Tick();
			}
			settings.Tick();
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
			restart.Dispose();
			menu.Dispose();
			cut1.Dispose();

			if (game.Type != GameType.EDITOR)
			{
				load.Dispose();
				save.Dispose();
				cut2.Dispose();
			}
			settings.Dispose();
			editor.Dispose();
			cut3.Dispose();
			leave.Dispose();
		}
	}
}
