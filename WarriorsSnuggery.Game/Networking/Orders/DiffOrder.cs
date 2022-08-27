using System;
using System.IO;

namespace WarriorsSnuggery.Networking.Orders
{
	internal class DiffOrder : IOrder
	{
		const NetworkPackageType type = NetworkPackageType.LOAD;

		public bool Immediate => false;

		readonly GameDiff diff;

		public readonly string SaveName = GameSaveManager.TempSaveName;

		public DiffOrder(GameDiff diff)
		{
			this.diff = diff;
		}

		public DiffOrder(byte[] data)
		{
			SaveName += Window.GlobalTick;
			const int intOffset = 4;

			var fileALength = BitConverter.ToInt32(data, 0);
			var index = intOffset;
			File.WriteAllBytes(FileExplorer.Saves + SaveName + ".yaml", data[index..(index + fileALength)]);
			var fileBLength = BitConverter.ToInt32(data, fileALength + intOffset);
			index = intOffset + fileALength + intOffset;
			File.WriteAllBytes(FileExplorer.Saves + SaveName + "_map.yaml", data[index..(index + fileBLength)]);
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
