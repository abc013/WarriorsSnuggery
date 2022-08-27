using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using WarriorsSnuggery.Networking.Orders;

namespace WarriorsSnuggery.Networking
{
	internal class RemoteClient
	{
		public int ID { get; private set; } = -1;

		readonly TcpClient client;
		readonly NetworkStream stream;

		readonly string password;

		readonly List<IOrder> dispatchOrders = new List<IOrder>();
		readonly List<IOrder> receivedOrders = new List<IOrder>();

		public bool IsPending { get; private set; } = true;
		public bool IsActive { get; private set; } = true;

		public RemoteClient(string address, int port, string password)
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
			if (!IsPending)
				return;

			var package = new NetworkPackage(stream);

			if (checkDisconnect(package))
				return;

			if (package.Type != NetworkPackageType.WELCOME)
				return;

			// Needs password
			if (NetworkUtils.ToString(package.Content) == "pwd?")
			{
				Log.Debug("(Networking) Password required. Sending password...");
				var response = new NetworkPackage(NetworkPackageType.WELCOME, NetworkUtils.ToBytes(password));
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

			receivedOrders.Add(package.ToOrder());
		}

		bool checkDisconnect(NetworkPackage package)
		{
			if (package.Type == NetworkPackageType.GOODBYE)
			{
				var message = NetworkUtils.ToString(package.Content);
				Log.Debug($"(Networking) Connection closed: {message}");
				Close();
				return true;
			}

			if (package.Type == NetworkPackageType.ERROR)
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

			// Connection has been surprisingly closed
			if (!stream.Socket.Connected)
			{
				IsActive = false;
				return false;
			}

			var package = order.GeneratePackage();
			stream.Write(package.AsBytes());
			return true;
		}

		public void Close()
		{
			var package = new NetworkPackage(NetworkPackageType.GOODBYE, new byte[0]);
			stream.Write(package.AsBytes());

			IsActive = false;
		}
	}
}
