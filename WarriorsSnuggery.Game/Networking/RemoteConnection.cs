using WarriorsSnuggery.Networking.Orders;

namespace WarriorsSnuggery.Networking
{
	public class RemoteConnection : IConnection
	{
		readonly Client client;

		public RemoteConnection(string address, int port, string password)
		{
			client = new Client(address, port, password);
		}

		public bool IsActive()
		{
			return client.IsActive && !client.IsPending;
		}

		public void Send(IOrder order)
		{
			client.Send(order);
		}

		public IOrder[] Receive()
		{
			return client.Receive();
		}

		public void Close()
		{
			client.Close();
		}
	}
}
