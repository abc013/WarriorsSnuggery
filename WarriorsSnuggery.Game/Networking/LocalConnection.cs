using System.Collections.Generic;
using WarriorsSnuggery.Networking.Orders;

namespace WarriorsSnuggery.Networking
{
	public class LocalConnection : IConnection
	{
		readonly List<IOrder> orderBuffer = new List<IOrder>();

		public LocalConnection()
		{

		}

		public bool IsActive()
		{
			return true;
		}

		public void Send(IOrder order)
		{
			orderBuffer.Add(order);
		}

		public IOrder[] Receive()
		{
			var array = orderBuffer.ToArray();
			orderBuffer.Clear();

			return array;
		}

		public void Close()
		{
			orderBuffer.Clear();
		}
	}
}
