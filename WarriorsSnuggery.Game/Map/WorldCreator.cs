using System.IO;
using System.Linq;

namespace WarriorsSnuggery.Maps
{
	public static class WorldCreator
	{
		public static Piece CreateEmpty(string name, MPos size)
		{
			var path = FileExplorer.Maps + @"\maps";

			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);

			using (var stream = new StreamWriter(FileExplorer.CreateFile(path + FileExplorer.Separator, name, ".yaml")))
			{
				stream.WriteLine("Name=" + name);
				stream.WriteLine("Size=" + size);

				var terrain = string.Join(",", Enumerable.Repeat("0", size.X * size.Y));
				stream.WriteLine("Terrain=" + terrain);

				var walls = string.Join(",", Enumerable.Repeat("-1", (size.X + 1) * (size.Y + 1) * 2 * 2));
				stream.WriteLine("Walls=" + walls);
			}

			// Load piece into cache, overwrite the old if there is one.
			return PieceManager.ReloadPiece(name);
		}
	}
}
