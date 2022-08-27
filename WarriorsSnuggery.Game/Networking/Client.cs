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

		readonly List<IOrder> dispatchOrders = new List<IOrder>();
		readonly List<IOrder> receivedOrders = new List<IOrder>();

		public bool IsPending { get; private set; } = true;
		public bool IsActive { get; private set; } = true;

		public Client(string address, int port, string password)
		{
			Log.Debug($"(Networking) Connecting to server at {address}:{port}.");
			client = new TcpClient(address, port) { NoDelay = true };
			stream = client.GetStream();
			this.password = password;

			new Thread(new ThreadStart(loop)) { IsBackground = true }.Start();
		}

		void loop()
		{
			while (IsActive && client.Connected)
			{
				// If pending, wait for possible answer.
				if (IsPending)
				{
					if (client.Available > 0)
						checkPending();

					continue;
				}

				while (client.Available > 0)
					receiveOrder();

				lock (dispatchOrders)
				{
					foreach (var order in dispatchOrders)
						dispatchOrder(order);
					dispatchOrders.Clear();
				}
			}
			IsActive = false;
		}

		void checkPending()
		{
			var package = new NetworkPackage(stream);

			if (!IsPending)
				return;

			if (checkDisconnect(package))
				return;

			if (package.Type != PackageType.WELCOME)
				return;

			// Needs password
			if (package.Content[0] == 1)
			{
				Log.Debug("(Networking) Password required. Sending password...");
				var response = new NetworkPackage(PackageType.WELCOME, NetworkUtils.ToBytes(password));
				stream.Write(response.AsBytes());
				return;
			}

			ID = BitConverter.ToInt32(package.Content, 1);

			Log.Debug($"(Networking) Connection established (ID: {ID}).");
			IsPending = false;
		}

		void receiveOrder()
		{
			var package = new NetworkPackage(client.GetStream());

			if (checkDisconnect(package))
				return;

			receivedOrders.Add(NetworkUtils.ToOrder(package));
		}

		bool checkDisconnect(NetworkPackage package)
		{
			if (package.Type == PackageType.GOODBYE)
			{
				var message = NetworkUtils.ToString(package.Content);
				Log.Debug($"(Networking) Connection closed: {message}");
				Close();
				return true;
			}

			if (package.Type == PackageType.ERROR)
			{
				var message = NetworkUtils.ToString(package.Content);
				Log.Debug($"(Networking) Server error. Error message: {message}");
				Close();
				return true;
			}

			return false;
		}

		public IOrder[] Receive()
		{
			var array = receivedOrders.ToArray();
			receivedOrders.Clear();

			return array;
		}

		public void Send(IOrder order)
		{
			if (order.Immediate)
			{
				dispatchOrder(order);
				return;
			}

			lock (dispatchOrders)
			{
				dispatchOrders.Add(order);
			}
		}

		bool dispatchOrder(IOrder order)
		{
			if (IsPending || !IsActive)
				return false;

			var package = order.GeneratePackage();
			stream.Write(package.AsBytes());
			return true;
		}

		public void Close()
		{
			var package = new NetworkPackage(PackageType.GOODBYE, new byte[0]);
			stream.Write(package.AsBytes());

			IsActive = false;
		}
	}
}
