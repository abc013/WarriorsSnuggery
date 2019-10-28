using System;
using WarriorsSnuggery.Maps;

namespace WarriorsSnuggery
{
	public static class GameController
	{
		static Game game;

		public static void Tick()
		{
			KeyInput.Tick();
			MouseInput.Tick();

			game.Tick();
		}

		public static void Load()
		{
			RuleLoader.LoadRules();
			RuleLoader.LoadUIRules();

			MapCreator.LoadTypes(FileExplorer.Maps, "maps.yaml");

			GameSaveManager.Load();
			GameSaveManager.DefaultStatistic = GameStatistics.LoadGameStatistic("DEFAULT");

			CreateNew(new GameStatistics(GameSaveManager.DefaultStatistic), GameType.MAINMENU);
		}

		public static void CreateNew(GameStatistics stats, GameType type = GameType.NORMAL, bool sameSeed = false, MapInfo custom = null, bool loadStatsMap = false)
		{
			if (game != null)
			{
				game.Finish();
				game.Dispose();
			}

			if (loadStatsMap)
			{
				try
				{
					custom = MapInfo.MapTypeFromSave(stats);
				}
				catch (System.IO.FileNotFoundException)
				{
					Log.WriteDebug(string.Format("Unable to load saved map of save '{0}'. Using a random map.", stats.SaveName));
				}
			}

			if (!sameSeed)
			{
				switch (type)
				{
					case GameType.TUTORIAL:
						game = new Game(stats, custom ?? MapCreator.FindTutorial());
						break;
					case GameType.MENU:
						game = new Game(stats, custom ?? MapCreator.FindMainMap(stats.Level));
						break;
					case GameType.MAINMENU:
						game = new Game(stats, custom ?? MapCreator.FindMainMenuMap(stats.Level));
						break;
					default:
						game = new Game(stats, custom ?? MapCreator.FindMap(stats.Level));
						break;
				}
			}
			else
			{
				game = new Game(stats, custom ?? game.MapType, game.Seed);
			}

			game.Load();
			if (stats.Health > 0 && game.World.LocalPlayer != null && game.World.LocalPlayer.Health != null)
				game.World.LocalPlayer.Health.HP = stats.Health;

			Camera.Reset();
			MasterRenderer.UpdateView();
		}

		public static void Pause()
		{
			if (!game.Paused)
				game.ChangeScreen(UI.ScreenType.PAUSED);

			game.Pause(true);
		}

		public static void Exit()
		{
			if (game != null)
			{
				game.Finish();
				game.Dispose();
			}
		}
	}
}
