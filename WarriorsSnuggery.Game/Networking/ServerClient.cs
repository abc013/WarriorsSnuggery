using System.Collections.Generic;
using System.Net.Sockets;

namespace WarriorsSnuggery.Networking
{
	internal class ServerClient
	{
		public readonly int ID;

		public bool Disconnecting;
		public bool Connected => client.Connected && !closed;
		bool closed;

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

			try
			{
				while (stream.DataAvailable)
					packages.Add(new NetworkPackage(stream));
			}
			catch
			{
				// In case of network error, close client.
				client.Close();
				closed = true;
			}

			return packages;
		}

		public void Send(NetworkPackage package)
		{
			if (!Connected)
				return;

			try
			{
				stream.Write(package.AsBytes());
			}
			catch
			{
				// In case of network error, close client.
				client.Close();
				closed = true;
			}
		}

		public void Disconnect(string message)
		{
			if (!Connected)
				return;

			var package = new NetworkPackage(NetworkPackageType.GOODBYE, NetworkUtils.ToBytes(message));

			Send(package);
			client.Close();
			closed = true;
		}
	}
}
