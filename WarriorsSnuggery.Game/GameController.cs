using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Maps;
using WarriorsSnuggery.Maps.Pieces;
using WarriorsSnuggery.Networking;
using WarriorsSnuggery.Networking.Orders;
using WarriorsSnuggery.UI.Screens;

namespace WarriorsSnuggery
{
	public static class GameController
	{
		public static bool ServerRunning { get; private set; }
		static Server localServer;

		public static bool RemoteConnection { get; private set; }
		static IConnection connection;
		// Used when remotely loading a map.
		static bool maintainConnection;

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

			ProcessReceivedOrders();
		}

		static void openServer(string address = NetworkUtils.DefaultAddress, int port = NetworkUtils.DefaultPort, string password = "", int playerCount = 10)
		{
			localServer = new Server(game, address, password, port, playerCount);
			ServerRunning = true;
			Connect(address, port, password);
		}

		public static bool Connect(string address = NetworkUtils.DefaultAddress, int port = NetworkUtils.DefaultPort, string password = "")
		{
			try
			{
				connection = new RemoteConnection(address, port, password);
				RemoteConnection = true;
			}
			catch (Exception ex)
			{
				Log.Warning($"(Networking) Failed to connect. Reason: {ex.Message}.");
				RemoteConnection = false;
			}

			return RemoteConnection;
		}

		public static void SendOrder(IOrder order)
		{
			connection.Send(order);
		}

		public static void ProcessReceivedOrders()
		{
			foreach (var order in connection.Receive())
			{
				switch (order)
				{
					case LoadOrder l:
						maintainConnection = true;
						CreateFromSave(new GameSave(FileExplorer.Saves + l.SaveName + ".yaml"));
						break;
					case DiffOrder d:
						// TODO replace with in-memory-loading
						maintainConnection = true;
						var save = new GameSave(FileExplorer.Saves + d.SaveName + ".yaml");
						CreateFromSave(save);
						save.Delete();
						break;
					case ChatOrder c:
						game.ScreenControl.ReceiveChat(c.Message);
						break;
					case PauseOrder p:
						game.ReceivePause(p.Paused);
						break;
					case PartyModeOrder p:
						Settings.PartyMode = p.PartyMode;

						if (!Settings.PartyMode)
							WorldRenderer.Ambient = game.MapType.Ambient;
						break;
				}
			}
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

			connection = new LocalConnection();

			if (!string.IsNullOrEmpty(Program.ServerAddress))
			{
				var split = Program.ServerAddress.Split(":");
				Connect(split[0], int.Parse(split[1]));
				return;
			}

			if (Program.StartServer)
				openServer();
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

			if (!maintainConnection)
			{
				connection.Close();
				connection = new LocalConnection();
				maintainConnection = false;
			}
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

			connection.Close();
			if (ServerRunning)
				localServer.Close();
		}
	}
}
