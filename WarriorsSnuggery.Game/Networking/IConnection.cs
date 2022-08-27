using WarriorsSnuggery.Networking.Orders;

namespace WarriorsSnuggery.Networking
{
	public interface IConnection
	{
		public abstract bool IsActive();

		public abstract void Send(IOrder order);

		public abstract IOrder[] Receive();

		public abstract void Close();
	}
}
