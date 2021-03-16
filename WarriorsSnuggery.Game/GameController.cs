using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
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
			var mission = MissionType.MAIN_MENU;
			var mode = InteractionMode.NONE;
			var map = MapCreator.FindMap(mission, 0, Program.SharedRandom);

			if (!string.IsNullOrEmpty(Program.Piece))
			{
				mode = InteractionMode.INGAME;
				map = MapType.FromPiece(PieceManager.GetPiece(Program.Piece));
				mission = MissionType.TEST;
			}
			else if (!string.IsNullOrEmpty(Program.MapType))
			{
				mode = InteractionMode.INGAME;
				map = MapCreator.GetType(Program.MapType);
				mission = map.MissionTypes.Length > 0 ? map.MissionTypes[0] : MissionType.TEST;
			}

			if (Program.StartEditor)
				mode = InteractionMode.EDITOR;


			game = new Game(new GameStatistics(GameSaveManager.DefaultStatistic), map, mission, mode);
			game.Load();
		}

		public static void CreateMainMenu()
		{
			const MissionType mission = MissionType.MAIN_MENU;

			finishAndLoad(new Game(GameSaveManager.DefaultStatistic, MapCreator.FindMap(mission, 0, Program.SharedRandom), mission, InteractionMode.NONE));
		}

		public static void CreateMenu()
		{
			var mission = game.MenuType;
			var stats = game.OldStatistics;

			finishAndLoad(new Game(stats, MapCreator.FindMap(mission, stats.Level, new Random(stats.Seed + stats.Level)), mission, InteractionMode.INGAME));
		}

		public static void CreateRestart()
		{
			var stats = game.OldStatistics;
			var mapType = game.MapType;

			// Don't start at last saved position, start right from the beginning
			if (mapType.IsSave)
				mapType = MapCreator.GetType(stats.CurrentMapType);

			finishAndLoad(new Game(stats, mapType, game.MissionType, game.InteractionMode, game.Seed));
		}

		public static void CreateNext()
		{
			var mission = game.CampaignType;
			var stats = game.Statistics;

			finishAndLoad(new Game(stats, MapCreator.FindMap(mission, stats.Level, new Random(stats.Seed + stats.Level)), mission, InteractionMode.INGAME));
		}

		public static void CreateNextMenu()
		{
			var mission = game.MenuType;
			var stats = game.Statistics;

			finishAndLoad(new Game(stats, MapCreator.FindMap(mission, stats.Level, new Random(stats.Seed + stats.Level)), mission, InteractionMode.INGAME));
		}

		public static void CreateNext(MissionType type, InteractionMode mode = InteractionMode.INGAME)
		{
			var stats = game.Statistics;

			finishAndLoad(new Game(stats, MapCreator.FindMap(type, stats.Level, new Random(stats.Seed + stats.Level)), type, mode));
		}

		public static void CreateNew(GameStatistics stats, MissionType type = MissionType.NORMAL, InteractionMode mode = InteractionMode.INGAME, MapType custom = null, bool loadStatsMap = false)
		{
			if (loadStatsMap)
			{
				type = stats.CurrentMission;
				try
				{
					custom = MapType.FromSave(stats);
				}
				catch (System.IO.FileNotFoundException)
				{
					Log.WriteDebug(string.Format("Unable to load saved map of save '{0}'. Using a random map.", stats.SaveName));
				}
			}

			finishAndLoad(new Game(stats, custom ?? MapCreator.FindMap(type, stats.Level, new Random(stats.Seed + stats.Level)), type, mode));
		}

		static void finishAndLoad(Game @new)
		{
			game.Finish();
			game.Dispose();

			game = @new;
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
