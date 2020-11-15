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

		readonly List<TcpClient> pending = new List<TcpClient>();
		readonly List<TcpClient> connected = new List<TcpClient>();

		readonly string name;
		readonly string password;
		bool requiresPassword => !string.IsNullOrEmpty(password);
		readonly int playerCount;

		public bool AllowConnections = true;

		bool isActive = true;

		public Server(string name, string password, int port = NetworkUtils.DefaultPort, int playerCount = 20)
		{
			server = new TcpListener(IPAddress.Any, port);
			this.name = name;
			this.password = password;
			this.playerCount = playerCount;

			new Thread(new ThreadStart(loop)).Start();
		}

		void loop()
		{
			server.Start();

			while (isActive)
			{
				if (AllowConnections)
				{
					if (server.Pending() && playerCount == connected.Count)
						connect(server.AcceptTcpClient());

					pending.RemoveAll(c => connected.Contains(c));

					foreach (var client in pending)
					{
						if (playerCount == connected.Count)
							continue;

						if (client.Available > 0)
							checkPending(client);
					}
				}

				foreach (var client in connected)
				{
					if (client.Available > 0)
						receive(client);
				}
				connected.RemoveAll(c => !c.Connected);
			}

			server.Stop();
		}

		void connect(TcpClient client)
		{
			Log.WriteDebug("New client detected.");
			pending.Add(client);

			if (requiresPassword)
			{
				Log.WriteDebug("Sending password request...");
				var package = new NetworkPackage(PackageType.WELCOME, BitConverter.GetBytes(!requiresPassword));
				client.GetStream().Write(package.AsBytes());
				return;
			}

			// No password required. We can send the data directly...
			checkPending(client);
		}

		void checkPending(TcpClient client)
		{
			var stream = client.GetStream();
			var bytes = NetworkUtils.ToBytes("hello");
			var data = new byte[bytes.Length + 1];
			data[0] = 0;
			Array.Copy(bytes, 0, data, 0, bytes.Length);
			Log.WriteDebug("Client connected. Sending data...");

			var package = new NetworkPackage(PackageType.WELCOME, data);
			stream.Write(package.AsBytes());

			connected.Add(client);
		}

		void receive(TcpClient client)
		{
			var package = new NetworkPackage(client.GetStream());

			if (package.Type == PackageType.GOODBYE)
			{
				Log.WriteDebug("Client closing. Closing connection...");
				client.Close();
				return;
			}

			Log.WriteDebug("Command reveived.");
		}

		public void Send()
		{

		}

		public void Close()
		{
			isActive = false;
		}
	}
}
