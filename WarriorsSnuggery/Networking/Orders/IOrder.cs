namespace WarriorsSnuggery.Networking.Orders
{
	public interface IOrder
	{
		bool Immediate { get; }

		NetworkPackage GeneratePackage();
	}
}
