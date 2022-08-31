using System;
using System.Collections.Generic;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Networking.Orders
{
	internal class LoadOrder : IOrder
	{
		const NetworkPackageType type = NetworkPackageType.LOAD;

		public bool Immediate => false;

		readonly GameSaveData diff;

		// Only for receiving
		public readonly List<TextNode> SaveNodes;
		public readonly List<TextNode> MapNodes;

		public LoadOrder(GameSaveData diff)
		{
			this.diff = diff;
		}

		public LoadOrder(byte[] data)
		{
			const int intOffset = 4;

			var fileALength = BitConverter.ToInt32(data, 0);
			var index = intOffset;
			SaveNodes = TextNodeLoader.FromArray(data[index..(index + fileALength)], "DiffOrder");
			var fileBLength = BitConverter.ToInt32(data, fileALength + intOffset);
			index = intOffset + fileALength + intOffset;
			MapNodes = TextNodeLoader.FromArray(data[index..(index + fileBLength)], "DiffOrder");
		}

		public NetworkPackage GeneratePackage()
		{
			const int intOffset = 4;

			var saveSize = diff.SaveData.Length;
			var mapSize = diff.MapData.Length;

			var array = new byte[saveSize + intOffset + mapSize + intOffset];
			var index = 0;

			Array.Copy(BitConverter.GetBytes(saveSize), array, intOffset);
			index += intOffset;
			Array.Copy(diff.SaveData, 0, array, index, saveSize);
			index += saveSize;
			Array.Copy(BitConverter.GetBytes(mapSize), 0, array, index, intOffset);
			index += intOffset;
			Array.Copy(diff.MapData, 0, array, index, mapSize);

			return new NetworkPackage(type, array);
		}
	}
}
