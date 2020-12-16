using System;

namespace WarriorsSnuggery.Networking.Orders
{
	class PauseOrder : IOrder
	{
		const PackageType type = PackageType.PAUSE;

		public bool Immediate => false;

		public readonly bool Paused;
		readonly byte[] data;

		public PauseOrder(bool paused)
		{
			Paused = paused;
			data = BitConverter.GetBytes(paused);
		}

		public PauseOrder(byte[] data)
		{
			Paused = data[0] == 1;
			this.data = data;
		}

		public NetworkPackage GeneratePackage()
		{
			return new NetworkPackage(type, data);
		}
	}
}
