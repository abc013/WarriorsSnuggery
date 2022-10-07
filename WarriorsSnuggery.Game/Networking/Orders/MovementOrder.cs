namespace WarriorsSnuggery.Networking.Orders
{
	internal class MovementOrder : IOrder
	{
		const NetworkPackageType type = NetworkPackageType.MOVEMENT;

		public bool Immediate => false;

		public readonly byte Vertical;
		public readonly byte Horizontal;

		public MovementOrder(byte vertical, byte horizontal)
		{
			Vertical = vertical;
			Horizontal = horizontal;
		}

		public MovementOrder(byte[] data)
		{
			Vertical = data[0];
			Horizontal = data[1];
		}

		public NetworkPackage GeneratePackage()
		{
			return new NetworkPackage(type, new[] { Vertical, Horizontal });
		}
	}
}
