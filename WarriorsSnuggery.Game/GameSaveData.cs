using System.Collections.Generic;
using System.IO;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery
{
	public class GameSaveData
	{
		public readonly byte[] SaveData;
		public readonly byte[] MapData;

		public readonly List<TextNode> SaveNodes;
		public readonly List<TextNode> MapNodes;

		public GameSaveData(Game game)
		{
			using var saveStream = new MemoryStream();
			using var mapStream = new MemoryStream();
			game.Save.Save(game, saveStream, mapStream);

			SaveData = saveStream.ToArray();
			MapData = mapStream.ToArray();
		}

		public GameSaveData(List<TextNode> saveNodes, List<TextNode> mapNodes)
		{
			SaveNodes = saveNodes;
			MapNodes = mapNodes;
		}
	}
}
