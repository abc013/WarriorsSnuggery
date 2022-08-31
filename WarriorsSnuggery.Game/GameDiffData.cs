using System.Collections.Generic;
using System.IO;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery
{
	public class GameDiffData
	{
		public readonly uint DiffTick;

		public readonly byte[] SaveData;
		public readonly byte[] MapData;

		public readonly List<TextNode> SaveNodes;
		public readonly List<TextNode> MapNodes;

		public GameDiffData(Game game, uint diffTick)
		{
			DiffTick = diffTick;

			using var saveStream = new MemoryStream();
			using var mapStream = new MemoryStream();
			game.Save.Diff(game, saveStream, mapStream);

			SaveData = saveStream.ToArray();
			MapData = mapStream.ToArray();
		}

		public GameDiffData(List<TextNode> saveNodes, List<TextNode> mapNodes)
		{
			SaveNodes = saveNodes;
			MapNodes = mapNodes;
		}
	}
}
