using WarriorsSnuggery.Networking.Orders;

namespace WarriorsSnuggery.Networking
{
	// Class will be used later for simulating networking in singleplayer
	public abstract class Connection
	{
		public abstract bool IsActive();

		public abstract void Send(IOrder order);

		public abstract IOrder[] Receive();

		public abstract void Close();
	}
}
