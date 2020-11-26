using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using WarriorsSnuggery.Networking.Orders;

namespace WarriorsSnuggery.Networking
{
	public class Client
	{
		readonly TcpClient client;
		readonly NetworkStream stream;

		readonly string password;

		readonly List<IOrder> orders = new List<IOrder>();

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
				// If pending, wait for possible answer.
				if (isPending)
				{
					if (client.Available > 0)
						checkPending();

					continue;
				}

				if (client.Available > 0)
					receiveOrder();

				foreach (var order in orders)
					dispatchOrder(order);
				orders.Clear();
			}
			isActive = false;
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
				var response = new NetworkPackage(PackageType.WELCOME, NetworkUtils.ToBytes(password));
				stream.Write(response.AsBytes());
				return;
			}

			// Server connection established
			Log.WriteDebug("Server package received. Connected.");
			isPending = false;
		}

		void receiveOrder()
		{
			var package = new NetworkPackage(client.GetStream());

			if (package.Type == PackageType.GOODBYE)
			{
				Log.WriteDebug("Server closing. Closing connection...");
				Close();
				return;
			}

			if (package.Type == PackageType.ERROR)
			{
				var message = NetworkUtils.ToString(package.Content);
				Log.WriteDebug($"Server error. Error message: {message}");
				Close();
				return;
			}

			if (package.Type == PackageType.MESSAGE)
			{
				GameController.AddInfoMessage(1000, NetworkUtils.ToString(package.Content));
			}

			Log.WriteDebug("Data received.");
		}

		public void Send(IOrder order)
		{
			if (order.Immediate)
			{
				dispatchOrder(order);
				return;
			}
			
			orders.Add(order);
		}

		bool dispatchOrder(IOrder order)
		{
			if (!isPending || !isActive)
				return false;

			var package = order.GeneratePackage();
			stream.Write(package.AsBytes());
			return true;
		}

		public void Close()
		{
			isActive = false;
		}
	}
}
