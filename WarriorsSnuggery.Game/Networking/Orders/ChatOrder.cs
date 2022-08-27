namespace WarriorsSnuggery.Networking.Orders
{
	class ChatOrder : IOrder
	{
		const NetworkPackageType type = NetworkPackageType.CHAT;

		public bool Immediate => true;

		public readonly string Message;
		readonly byte[] data;

		public ChatOrder(string message)
		{
			Message = message;
			data = NetworkUtils.ToBytes(message);
		}

		public ChatOrder(byte[] data)
		{
			Message = NetworkUtils.ToString(data);
			this.data = data;
		}

		public NetworkPackage GeneratePackage()
		{
			return new NetworkPackage(type, data);
		}
	}
}
