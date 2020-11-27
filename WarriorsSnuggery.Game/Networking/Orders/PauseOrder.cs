using System;

namespace WarriorsSnuggery.Networking.Orders
{
	class PauseOrder : IOrder
	{
		const PackageType type = PackageType.PAUSE;

		public bool Immediate => false;

		readonly byte[] data;

		public PauseOrder(bool paused)
		{
			data = BitConverter.GetBytes(paused);
		}

		public NetworkPackage GeneratePackage()
		{
			return new NetworkPackage(type, data);
		}
	}
}
