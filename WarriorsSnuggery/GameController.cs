﻿using OpenTK.Windowing.GraphicsLibraryFramework;
using WarriorsSnuggery.Maps;
using WarriorsSnuggery.UI.Screens;

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

			PieceManager.RefreshPieces();

			MapCreator.LoadMaps(FileExplorer.Maps, "maps.yaml");

			GameSaveManager.Load();
			GameSaveManager.DefaultStatistic = new GameStatistics("DEFAULT");

			createFirst();
		}

		static void createFirst()
		{
			var type = GameType.MAINMENU;
			var map = MapCreator.FindMap(type, 0);

			if (!string.IsNullOrEmpty(Program.MapType))
			{
				map = MapCreator.GetType(Program.MapType);
				type = map.DefaultType;
			}

			game = new Game(new GameStatistics(GameSaveManager.DefaultStatistic), map, type);
			game.Load();

			if (Program.StartEditor)
				game.SwitchEditor();
		}

		public static void CreateReturn(GameType type)
		{
			var stats = game.OldStatistics;

			game.Finish();
			game.Dispose();

			game = new Game(stats, MapCreator.FindMap(type, stats.Level), type);
			game.Load();
		}

		public static void CreateRestart()
		{
			var stats = game.OldStatistics;

			game.Finish();
			game.Dispose();

			game = new Game(stats, game.MapType, game.Type, game.Seed);
			game.Load();
		}

		public static void CreateNext(GameType type)
		{
			var stats = game.Statistics;

			game.Finish();
			game.Dispose();

			game = new Game(stats, MapCreator.FindMap(type, stats.Level), type);
			game.Load();
		}

		public static void CreateNew(GameStatistics stats, GameType type = GameType.NORMAL, MapInfo custom = null, bool loadStatsMap = false)
		{
			game.Finish();
			game.Dispose();

			if (loadStatsMap)
			{
				type = stats.CurrentType;
				try
				{
					custom = MapInfo.FromSave(stats);
				}
				catch (System.IO.FileNotFoundException)
				{
					Log.WriteDebug(string.Format("Unable to load saved map of save '{0}'. Using a random map.", stats.SaveName));
				}
			}

			game = new Game(stats, custom ?? MapCreator.FindMap(type, stats.Level), type);
			game.Load();
		}

		public static void AddInfoMessage(int duration, string text)
		{
			game.AddInfoMessage(duration, text);
		}

		public static void Pause()
		{
			if (!game.Paused)
				game.ShowScreen(ScreenType.PAUSED);

			game.Pause(true);
		}

		public static void KeyDown(Keys key, bool isControl, bool isShift, bool isAlt)
		{
			game.KeyDown(key, isControl, isShift, isAlt);
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
