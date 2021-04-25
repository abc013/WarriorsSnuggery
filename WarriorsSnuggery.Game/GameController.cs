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
			Loader.RuleLoader.LoadRules();

			PieceManager.RefreshPieces();

			MapCreator.LoadMaps(FileExplorer.Maps, "maps.yaml");

			GameSaveManager.Load();

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


			game = new Game(GameSaveManager.DefaultSave.Copy(), map, mission, mode);
			game.Load();
		}

		public static void CreateMainMenu()
		{
			const MissionType mission = MissionType.MAIN_MENU;

			finishAndLoad(new Game(GameSaveManager.DefaultSave, MapCreator.FindMap(mission, 0, Program.SharedRandom), mission, InteractionMode.NONE));
		}

		public static void CreateMenu()
		{
			var mission = game.MenuType;
			var save = game.Save;

			finishAndLoad(new Game(save, MapCreator.FindMap(mission, save), mission, InteractionMode.INGAME));
		}

		public static void CreateRestart()
		{
			var save = game.Save;
			var mapType = game.MapType;

			// Don't start at last saved position, start right from the beginning
			if (mapType.IsSave)
				mapType = save.CurrentMapType;

			finishAndLoad(new Game(save, mapType, game.MissionType, game.InteractionMode, game.Seed));
		}

		public static void CreateNext()
		{
			var mission = game.CampaignType;
			var save = game.Save;

			finishAndLoad(new Game(save, MapCreator.FindMap(mission, save), mission, InteractionMode.INGAME));
		}

		public static void CreateNextMenu()
		{
			var mission = game.MenuType;
			var save = game.Save;

			finishAndLoad(new Game(save, MapCreator.FindMap(mission, save), mission, InteractionMode.INGAME));
		}

		public static void CreateNext(MissionType mission, InteractionMode mode = InteractionMode.INGAME)
		{
			var save = game.Save;

			finishAndLoad(new Game(save, MapCreator.FindMap(mission, save), mission, mode));
		}

		public static void CreateNew(GameSave save, MissionType type = MissionType.NORMAL, InteractionMode mode = InteractionMode.INGAME, MapType custom = null, bool loadStatsMap = false)
		{
			if (loadStatsMap)
			{
				type = save.CurrentMission;
				try
				{
					custom = MapType.FromSave(save);
				}
				catch (System.IO.FileNotFoundException)
				{
					Log.WriteDebug(string.Format("Unable to load saved map of save '{0}'. Using a random map.", save.SaveName));
				}
			}

			finishAndLoad(new Game(save, custom ?? MapCreator.FindMap(type, save.Level, new Random(save.Seed + save.Level)), type, mode));
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
