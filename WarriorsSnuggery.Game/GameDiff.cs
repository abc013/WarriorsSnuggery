using System.Collections.Generic;
using System.IO;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery
{
    public class GameDiff
    {
        public readonly uint DiffTick;

        public readonly byte[] SaveData;
        public readonly byte[] MapData;

        public readonly List<TextNode> SaveNodes;
        public readonly List<TextNode> MapNodes;

        public GameDiff(Game game, uint diffTick, bool fullSave = false)
        {
            DiffTick = diffTick;

            using var saveStream = new MemoryStream();
            using var mapStream = new MemoryStream();
            if (fullSave)
                game.Save.Save(game, saveStream, mapStream);
            else
                game.Save.Diff(game, saveStream, mapStream);

            SaveData = saveStream.ToArray();
            MapData = mapStream.ToArray();
        }

        public GameDiff(List<TextNode> saveNodes, List<TextNode> mapNodes)
        {
            SaveNodes = saveNodes;
            MapNodes = mapNodes;
        }
    }
}
