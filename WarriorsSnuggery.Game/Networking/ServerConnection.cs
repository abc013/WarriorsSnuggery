using WarriorsSnuggery.Networking.Orders;

namespace WarriorsSnuggery.Networking
{
	public class ServerConnection : Connection
	{
		readonly Client client;

		public ServerConnection(string address, int port, string password)
		{
			client = new Client(address, port, password);
		}

		public override bool IsActive()
		{
			return client.IsActive && !client.IsPending;
		}

		public override void Send(IOrder order)
		{
			client.Send(order);
		}

		public override IOrder[] Receive()
		{
			return client.Receive();
		}

		public override void Close()
		{
			client.Close();
		}
	}
}
