using System.IO;

namespace WarriorsSnuggery
{
    public class GameDiff
    {
        public readonly uint DiffTick;

        public readonly byte[] SaveData;
        public readonly byte[] MapData;

        public GameDiff(Game game, uint diffTick)
        {
            DiffTick = diffTick;

            using var saveStream = new MemoryStream();
            using var mapStream = new MemoryStream();
            game.Save.SaveInMemory(game, saveStream, mapStream);

            SaveData = saveStream.ToArray();
            MapData = mapStream.ToArray();
        }
    }
}
