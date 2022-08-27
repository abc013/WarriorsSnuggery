using System;

namespace WarriorsSnuggery.Networking.Orders
{
	class PartyModeOrder : IOrder
	{
		const NetworkPackageType type = NetworkPackageType.PARTYMODE;

		public bool Immediate => false;

		public readonly bool PartyMode;
		readonly byte[] data;

		public PartyModeOrder(bool partyMode)
		{
			PartyMode = partyMode;
			data = BitConverter.GetBytes(partyMode);
		}

		public PartyModeOrder(byte[] data)
		{
			PartyMode = data[0] == 1;
			this.data = data;
		}

		public NetworkPackage GeneratePackage()
		{
			return new NetworkPackage(type, data);
		}
	}
}
