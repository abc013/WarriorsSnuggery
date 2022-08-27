using System;
using System.Collections.Generic;
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

		readonly string name;
		readonly string password;
		bool requiresPassword => !string.IsNullOrEmpty(password);

		readonly int playerCount;
		int nextID = 0;

		public bool AllowConnections = true;

		bool isActive = true;

		public Server(string name, string password, int port = NetworkUtils.DefaultPort, int playerCount = 20)
		{
			server = new TcpListener(IPAddress.Any, port);
			this.name = name;
			this.password = password;
			this.playerCount = playerCount;

			new Thread(new ThreadStart(loop)) { IsBackground = true }.Start();
		}

		void loop()
		{
			server.Start();

			while (isActive)
			{
				if (AllowConnections)
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

				foreach (var client in connected)
				{
					if (!client.Connected || !client.PackageAvailable)
						continue;

					foreach(var package in client.GetPackages())
						receive(client, package);
				}
				connected.RemoveAll(c => !c.Connected);
			}

			foreach (var client in pending)
				client.Disconnect("Server closing.");

			foreach (var client in connected)
				client.Disconnect("Server closing.");

			server.Stop();
		}

		void connect(ServerClient client)
		{
			Log.WriteDebug($"New client detected (ID: {client.ID}).");

			pending.Add(client);

			if (requiresPassword)
			{
				Log.WriteDebug("Sending password request...");
				client.Send(new NetworkPackage(PackageType.WELCOME, new byte[] { 1 }));
				return;
			}

			// No password required. We can send the data directly...
			accept(client);
		}

		void checkPassword(ServerClient client, NetworkPackage package)
		{
			if (password != NetworkUtils.ToString(package.Content))
			{
				Log.WriteDebug($"Client {client.ID}: disconnected. wrong password.");
				client.Disconnect("Wrong password.");
				return;
			}

			accept(client);
		}

		void accept(ServerClient client)
		{
			Log.WriteDebug($"Client {client.ID}: connected. Sending data...");

			var bytes = BitConverter.GetBytes(client.ID);
			var data = new byte[bytes.Length + 1];
			data[0] = 0;
			Array.Copy(bytes, 0, data, 1, bytes.Length);

			client.Send(new NetworkPackage(PackageType.WELCOME, data));

			connected.Add(client);
		}

		void receive(ServerClient client, NetworkPackage package)
		{
			switch (package.Type)
			{
				case PackageType.GOODBYE:
					Log.WriteDebug($"Client {client.ID}: Closing.");
					client.Disconnect("Client requested disconnect.");
					return;
				case PackageType.CHAT:
					var msg = NetworkUtils.ToString(package.Content);
					Log.WriteDebug($"Client {client.ID}: Message received: {msg}");
					broadcast(package);
					break;
				case PackageType.PAUSE:
					var pause = package.Content[0] == 1;
					Log.WriteDebug($"Client {client.ID}: Requested {(pause ? "" : "un")}pause.");
					broadcast(package);
					break;
			}
		}

		void broadcast(NetworkPackage package)
		{
			foreach (var client in connected)
				send(client, package);
		}

		void send(ServerClient client, NetworkPackage package)
		{
			client.Send(package);
		}

		public void Close()
		{
			isActive = false;
		}

		class ServerClient
		{
			public readonly int ID;

			public bool Connected => client.Connected;

			public bool PackageAvailable => stream.DataAvailable;

			readonly TcpClient client;
			readonly NetworkStream stream;

			public ServerClient(int id, TcpClient client)
			{
				ID = id;
				this.client = client;

				client.NoDelay = true;

				stream = client.GetStream();
			}

			public List<NetworkPackage> GetPackages()
			{
				var packages = new List<NetworkPackage>();

				if (!Connected)
					return packages;

				while (stream.DataAvailable)
					packages.Add(new NetworkPackage(stream));

				return packages;
			}

			public void Send(NetworkPackage package)
			{
				if (!Connected)
					return;

				stream.Write(package.AsBytes());
			}

			public void Disconnect(string message)
			{
				if (!Connected)
					return;

				var package = new NetworkPackage(PackageType.GOODBYE, NetworkUtils.ToBytes(message));

				Send(package);
				client.Close();
			}
		}
	}
}
