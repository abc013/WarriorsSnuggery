using System;
using WarriorsSnuggery.Networking;
using WarriorsSnuggery.Networking.Orders;

namespace WarriorsSnuggery
{
	public class OrderProcessor
	{
		public static bool ServerRunning { get; private set; }
		static Server localServer;

		public static bool RemoteConnection { get; private set; }
		static IConnection connection;
		// Used when remotely loading a map.
		static bool maintainConnection;

		public static void Load(Game game)
		{
			connection = new LocalConnection();

			if (!string.IsNullOrEmpty(Program.ServerAddress))
			{
				var split = Program.ServerAddress.Split(":");
				Connect(split[0], int.Parse(split[1]));
				return;
			}

			if (Program.StartServer)
				openServer(game);
		}

		static void openServer(Game game, string address = NetworkUtils.DefaultAddress, int port = NetworkUtils.DefaultPort, string password = "", int playerCount = 10)
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

		public static void ProcessReceivedOrders(Game game)
		{
			foreach (var order in connection.Receive())
			{
				switch (order)
				{
					case LoadOrder l:
						maintainConnection = true;
						GameController.CreateFromSave(new GameSave(l.SaveNodes, GameSaveManager.TempSaveName, l.MapNodes));
						break;
					case DiffOrder d:
						maintainConnection = true;
						// TODO: don't load!
						GameController.CreateFromSave(new GameSave(d.SaveNodes, GameSaveManager.TempSaveName, d.MapNodes));
						var diff = new GameDiff(d.SaveNodes, d.MapNodes);
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

		public static void LoadNext()
		{
			if (!maintainConnection)
			{
				connection.Close();
				connection = new LocalConnection();
				maintainConnection = false;
			}
		}

		public static void Exit()
		{
			connection.Close();
			if (ServerRunning)
				localServer.Close();
		}
	}
}
