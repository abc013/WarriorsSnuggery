namespace WarriorsSnuggery.Networking.Orders
{
	class ChatOrder : IOrder
	{
		const PackageType type = PackageType.MESSAGE;

		public bool Immediate => true;

		readonly byte[] data;

		public ChatOrder(string message)
		{
			data = NetworkUtils.ToBytes(message);
		}

		public NetworkPackage GeneratePackage()
		{
			return new NetworkPackage(type, data);
		}
	}
}
