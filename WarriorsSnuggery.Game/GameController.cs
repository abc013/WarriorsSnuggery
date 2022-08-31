using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Maps;
using WarriorsSnuggery.Maps.Pieces;
using WarriorsSnuggery.UI.Screens;

namespace WarriorsSnuggery
{
	public static class GameController
	{
		static Game game;
		static Game nextGame;

		public static bool NextGamePrepared => nextGame != null;

		public static void Load()
		{
			RuleLoader.Load();
			PieceManager.Load();

			GameSaveManager.Load();
		}

		public static void Tick()
		{
			game.Tick();

			OrderProcessor.ProcessReceivedOrders(game);
		}

		public static void CreateFirst()
		{
			var mission = MissionType.MAIN_MENU;
			var mode = InteractionMode.NONE;
			var map = MapCache.FindMap(mission, 0, Program.SharedRandom);

			if (!string.IsNullOrEmpty(Program.Piece))
			{
				mode = InteractionMode.INGAME;
				map = MapType.FromPiece(PieceManager.GetPiece(new PackageFile(Program.Piece)));
				mission = MissionType.TEST;
			}
			else if (!string.IsNullOrEmpty(Program.MapType))
			{
				mode = InteractionMode.INGAME;
				map = MapCache.Types[Program.MapType];
				mission = map.MissionTypes.Length > 0 ? map.MissionTypes[0] : MissionType.TEST;
			}

			if (Program.StartEditor)
				mode = InteractionMode.EDITOR;

			game = new Game(GameSaveManager.DefaultSave.Clone(), map, mission, mode);
			game.Load();

			OrderProcessor.CreateFirst(game);
		}

		public static void CreateMainMenu()
		{
			const MissionType mission = MissionType.MAIN_MENU;

			finishAndLoad(new Game(GameSaveManager.DefaultSave, MapCache.FindMap(mission, 0, Program.SharedRandom), mission, InteractionMode.NONE));
		}

		public static void CreateMenu()
		{
			var mission = game.MissionType.GetMenuType();
			var save = game.Save;

			finishAndLoad(new Game(save, MapCache.FindMap(mission, save), mission, InteractionMode.INGAME));
		}

		public static void CreateRestart()
		{
			var save = game.OriginalSave;
			var mapType = game.MapType;

			// Don't start at last saved position, start right from the beginning
			if (mapType.IsSave)
				mapType = save.CurrentMapType;

			finishAndLoad(new Game(save, mapType, game.MissionType, game.InteractionMode, game.Seed));
		}

		public static void CreateNext()
		{
			var mission = game.MissionType.GetCampaignType();
			var save = game.Save;

			finishAndLoad(new Game(save, MapCache.FindMap(mission, save), mission, InteractionMode.INGAME));
		}

		public static void CreateNextMenu()
		{
			var mission = game.MissionType.GetMenuType();
			var save = game.Save;

			finishAndLoad(new Game(save, MapCache.FindMap(mission, save), mission, InteractionMode.INGAME));
		}

		public static void CreateNext(MissionType mission, InteractionMode mode = InteractionMode.INGAME)
		{
			var save = game.Save;

			finishAndLoad(new Game(save, MapCache.FindMap(mission, save), mission, mode));
		}

		public static void CreateFromSave(GameSave save)
		{
			var type = save.CurrentMission;
			MapType custom = null;
			try
			{
				custom = MapType.FromSave(save);
			}
			catch (System.IO.FileNotFoundException)
			{
				Log.Warning($"Unable to load saved map of save '{save.SaveName}'. Using a random map.");
			}

			CreateNew(save, type: type, custom: custom);
		}

		public static void CreateNew(GameSave save, MissionType type = MissionType.NORMAL, InteractionMode mode = InteractionMode.INGAME, MapType custom = null)
		{
			finishAndLoad(new Game(save, custom ?? MapCache.FindMap(type, save.Level, new Random(save.Seed + save.Level)), type, mode));
		}

		static void finishAndLoad(Game @new)
		{
			nextGame = @new;
			game.Finish();
		}

		public static void LoadNext()
		{
			game.Dispose();

			game = nextGame;
			game.Load();

			nextGame = null;

			OrderProcessor.LoadNext();
		}

		public static void Pause()
		{
			if (game.Paused)
				return;

			game.ShowScreen(ScreenType.PAUSED);
			game.Pause(true);
		}

		public static void KeyDown(Keys key, bool isControl, bool isShift, bool isAlt)
		{
			if (isControl && key == Keys.P)
			{
				MasterRenderer.CreateScreenshot();
				game.AddInfoMessage(150, "Screenshot!");
			}

			game.KeyDown(key, isControl, isShift, isAlt);
		}

		public static void Exit()
		{
			if (game != null)
			{
				game.Finish();
				game.Dispose();
			}

			OrderProcessor.Exit();
		}
	}
}
