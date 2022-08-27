using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace WarriorsSnuggery.Networking
{
	public class Server
	{
		readonly TcpListener server;

		readonly List<ServerClient> pending = new List<ServerClient>();
		readonly List<ServerClient> connected = new List<ServerClient>();

		readonly Game game;
		ServerClient localClient;

		readonly string name;
		readonly string password;
		bool requiresPassword => !string.IsNullOrEmpty(password);

		readonly int playerCount;
		int nextID = 0;

		public bool AllowConnections = true;

		bool isActive = true;

		public Server(Game game, string name, string password, int port = NetworkUtils.DefaultPort, int playerCount = 20)
		{
			Log.Debug($"(Networking) Starting server: {name} running at port {port}.");

			this.game = game;
			this.name = name;
			this.password = password;
			this.playerCount = playerCount;

			server = new TcpListener(IPAddress.Any, port);
			server.Start(playerCount);

			new Thread(new ThreadStart(loop)) { IsBackground = true }.Start();
		}

		void loop()
		{
			while (isActive)
			{
				if (AllowConnections)
					checkPendingConnections();

				var diffOrder = new Orders.DiffOrder(game.AwaitGameDiff());
				var diffPackage = diffOrder.GeneratePackage();

				foreach (var client in connected)
				{
					if (!client.Connected)
					{
						client.Disconnecting = true;
						Log.Debug($"(Networking) Client {client.ID}: Disconnected.");
						broadcastMessage($"Client (ID {client.ID}) has disconnected.");

						continue;
					}

					try
					{
						if (client != localClient)
							client.Send(diffPackage);

						if (!client.PackageAvailable)
							continue;

						foreach (var package in client.GetPackages())
							receive(client, package);
					}
					catch(Exception ex)
					{
						Log.Warning($"(Networking) Unexpected error while talking to Client {client.ID}: {ex.Message}");
					}
				}

				connected.RemoveAll(c => c.Disconnecting);
			}

			foreach (var client in pending)
				client.Disconnect("Server closing.");

			foreach (var client in connected)
				client.Disconnect("Server closing.");

			server.Stop();
		}

		void checkPendingConnections()
		{
			if (server.Pending())
				connect(new ServerClient(nextID++, server.AcceptTcpClient()));

			pending.RemoveAll(c => connected.Contains(c) || !c.Connected);

			foreach (var client in pending)
			{
				if (playerCount == connected.Count)
				{
					client.Disconnect("Server is full.");
					continue;
				}

				if (!client.PackageAvailable)
					continue;

				var packages = client.GetPackages();

				if (packages.Count > 1)
				{
					client.Disconnect("Received more than one packages while pending.");
					continue;
				}

				checkPassword(client, packages[0]);
			}
		}

		void connect(ServerClient client)
		{
			Log.Debug($"(Networking) New client detected (ID: {client.ID}).");
			pending.Add(client);

			if (requiresPassword)
			{
				Log.Debug("(Networking) Sending password request...");
				client.Send(new NetworkPackage(NetworkPackageType.WELCOME, NetworkUtils.ToBytes("pwd?")));
			}
			else
			{
				// No password required. We can send the data directly...
				accept(client);
			}
		}

		void checkPassword(ServerClient client, NetworkPackage package)
		{
			if (password != NetworkUtils.ToString(package.Content))
			{
				Log.Debug($"(Networking) Client {client.ID}: disconnected. wrong password.");
				client.Disconnect("Wrong password.");
				return;
			}

			accept(client);
		}

		void accept(ServerClient client)
		{
			Log.Debug($"(Networking) Client {client.ID}: connected. Sending data...");

			var bytes = BitConverter.GetBytes(client.ID);
			var data = new byte[bytes.Length + 1];
			data[0] = 0;
			Array.Copy(bytes, 0, data, 1, bytes.Length);

			client.Send(new NetworkPackage(NetworkPackageType.WELCOME, data));

			connected.Add(client);

			// First connected client is the local client. Otherwise, send world state
			if (localClient == null)
				localClient = client;
			else
			{
				// HACK: Wait until the next save game has been generated
				game.ForceTempSave = true;
				while (game.ForceTempSave) ;

				client.Send(new Orders.LoadOrder(game).GeneratePackage());
			}

			broadcastMessage($"Client (ID {client.ID}) has connected.");
			if (localClient == client)
				broadcastMessage($"Client (ID {client.ID}) is local client.");
		}

		void receive(ServerClient client, NetworkPackage package)
		{
			switch (package.Type)
			{
				case NetworkPackageType.GOODBYE:
					Log.Debug($"(Networking) Client {client.ID}: Client requested disconnect.");
					client.Disconnect("Client requested disconnect.");
					return;
				case NetworkPackageType.CHAT:
					var msg = NetworkUtils.ToString(package.Content);
					Log.Debug($"(Networking) Client {client.ID}: Message received: {msg}");
					broadcast(package);
					break;
				case NetworkPackageType.PAUSE:
					var pause = package.Content[0] == 1;
					Log.Debug($"(Networking) Client {client.ID}: Requested {(pause ? "" : "un")}pause.");
					broadcast(package);
					break;
				case NetworkPackageType.PARTYMODE:
					var partymode = package.Content[0] == 1;
					Log.Debug($"(Networking) Client {client.ID}: Requested {(partymode ? "en" : "dis")}abling Partymode.");
					broadcast(package);
					break;
			}
		}

		void broadcastMessage(string message)
		{
			var text = $"{Color.Yellow}<Server> {message}";
			var package = new Orders.ChatOrder(text).GeneratePackage();
			broadcast(package);
		}

		void broadcast(NetworkPackage package)
		{
			foreach (var client in connected)
				client.Send(package);
		}

		public void Close()
		{
			isActive = false;
		}
	}
}
