using System;
using System.Collections.Generic;
using System.IO;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Networking.Orders
{
	internal class LoadOrder : IOrder
	{
		const NetworkPackageType type = NetworkPackageType.LOAD;

		public bool Immediate => false;

		readonly Game game;
		public readonly string SaveName = GameSaveManager.TempSaveName;
		public readonly List<TextNode> SaveNodes;
		public readonly List<TextNode> MapNodes;

		public LoadOrder(Game game)
		{
			this.game = game;
		}

		public LoadOrder(byte[] data)
		{
			const int intOffset = 4;

			var fileALength = BitConverter.ToInt32(data, 0);
			var index = intOffset;
			SaveNodes = TextNodeLoader.FromArray(data[index..(index + fileALength)], "LoadOrder");
			var fileBLength = BitConverter.ToInt32(data, fileALength + intOffset);
			index = intOffset + fileALength + intOffset;
			MapNodes = TextNodeLoader.FromArray(data[index..(index + fileBLength)], "LoadOrder");
		}

		public NetworkPackage GeneratePackage()
		{
			static byte[] concat(byte[] fileA, byte[] fileB)
			{
				var fileALength = BitConverter.GetBytes(fileA.Length);
				var fileBLength = BitConverter.GetBytes(fileB.Length);

				var array = new byte[fileA.Length + fileALength.Length + fileB.Length + fileBLength.Length];

				var index = 0;
				Array.Copy(fileALength, array, fileALength.Length);
				index += fileALength.Length;
				Array.Copy(fileA, 0, array, index, fileA.Length);
				index += fileA.Length;
				Array.Copy(fileBLength, 0, array, index, fileBLength.Length);
				index += fileBLength.Length;
				Array.Copy(fileB, 0, array, index, fileB.Length);

				return array;
			}

			var fileA = File.ReadAllBytes(FileExplorer.Saves + SaveName + ".yaml");
			var fileB = File.ReadAllBytes(FileExplorer.Saves + SaveName + "_map.yaml");

			return new NetworkPackage(type, concat(fileA, fileB));
		}
	}
}
