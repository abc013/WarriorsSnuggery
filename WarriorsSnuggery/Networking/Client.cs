using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using WarriorsSnuggery.Networking.Orders;

namespace WarriorsSnuggery.Networking
{
	public class Client
	{
		public int ID { get; private set; } = -1;

		readonly TcpClient client;
		readonly NetworkStream stream;

		readonly string password;

		readonly List<IOrder> orders = new List<IOrder>();

		public bool GameReady = false;
		bool isPending = true;
		bool isActive = true;

		public Client(string address = NetworkUtils.DefaultAddress, int port = NetworkUtils.DefaultPort, string password = "")
		{
			Log.WriteDebug("Connecting to server...");
			client = new TcpClient(address, port) { NoDelay = true };
			stream = client.GetStream();
			this.password = password;

			new Thread(new ThreadStart(loop)) { IsBackground = true }.Start();
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

				while (client.Available > 0)
					receiveOrder();

				lock (orders)
				{
					foreach (var order in orders)
						dispatchOrder(order);
					orders.Clear();
				}
			}
			isActive = false;
		}

		void checkPending()
		{
			var package = new NetworkPackage(stream);

			if (!isPending)
				return;

			if (checkDisconnect(package))
				return;

			if (package.Type != PackageType.WELCOME)
				return;

			// Needs password
			if (package.Content[0] == 1)
			{
				Log.WriteDebug("Password required. Sending password...");
				var response = new NetworkPackage(PackageType.WELCOME, NetworkUtils.ToBytes(password));
				stream.Write(response.AsBytes());
				return;
			}

			ID = BitConverter.ToInt32(package.Content, 1);

			// Server connection established
			Log.WriteDebug($"Connection established (ID: {ID}).");
			isPending = false;
		}

		void receiveOrder()
		{
			var package = new NetworkPackage(client.GetStream());

			if (checkDisconnect(package))
				return;

			if (!GameReady)
				return;

			GameController.Receive(package);
		}

		bool checkDisconnect(NetworkPackage package)
		{
			if (package.Type == PackageType.GOODBYE)
			{
				var message = NetworkUtils.ToString(package.Content);
				Log.WriteDebug($"Connection closed: {message}");
				Close();
				return true;
			}

			if (package.Type == PackageType.ERROR)
			{
				var message = NetworkUtils.ToString(package.Content);
				Log.WriteDebug($"Server error. Error message: {message}");
				Close();
				return true;
			}

			return false;
		}

		public void Send(IOrder order)
		{
			if (order.Immediate)
			{
				dispatchOrder(order);
				return;
			}

			lock (orders)
			{
				orders.Add(order);
			}
		}

		bool dispatchOrder(IOrder order)
		{
			if (isPending || !isActive)
				return false;

			var package = order.GeneratePackage();
			stream.Write(package.AsBytes());
			return true;
		}

		public void Close()
		{
			var package = new NetworkPackage(PackageType.GOODBYE, new byte[0]);
			stream.Write(package.AsBytes());

			isActive = false;
		}
	}
}
