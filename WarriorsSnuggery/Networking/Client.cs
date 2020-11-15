using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace WarriorsSnuggery.Networking
{
	public class Client
	{
		readonly TcpClient client;
		readonly NetworkStream stream;

		readonly string password;

		bool isPending = true;
		bool isActive = true;

		public Client(string address = NetworkUtils.DefaultAddress, int port = NetworkUtils.DefaultPort, string password = "")
		{
			Log.WriteDebug("Connecting to server...");
			client = new TcpClient(address, port) { NoDelay = true };
			stream = client.GetStream();
			this.password = password;

			new Thread(new ThreadStart(loop)).Start();
		}

		void loop()
		{
			while (isActive && client.Connected)
			{
				if (client.Available > 0)
				{
					if (isPending)
						checkPending();
					else
						receive();
				}
			}
		}

		void checkPending()
		{
			var package = new NetworkPackage(stream);

			if (package.Type != PackageType.WELCOME)
				return;

			// Needs password
			if (package.Content[0] == 1)
			{
				Log.WriteDebug("Server package received. Password required. Sending password...");
				var response = new NetworkPackage(PackageType.WELCOME, Encoding.ASCII.GetBytes(password));
				stream.Write(response.AsBytes());
				return;
			}

			// Server connection established
			Log.WriteDebug("Server package received. Connected.");
			isPending = false;
		}

		void receive()
		{
			var package = new NetworkPackage(client.GetStream());

			if (package.Type == PackageType.GOODBYE)
			{
				Log.WriteDebug("Server closing. Closing connection...");
				Close();
				return;
			}

			Log.WriteDebug("Data received.");
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
