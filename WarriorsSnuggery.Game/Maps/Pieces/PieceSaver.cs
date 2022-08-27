using System.IO;
using System.Linq;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Maps.Pieces
{
	public static class PieceSaver
	{
		public static Piece SaveEmpty(MPos size, string name)
		{
			var filepath = PackageManager.Core.PiecesDirectory + name + ".yaml";

			using (var stream = new StreamWriter(File.Create(filepath)))
			{
				stream.WriteLine("MapFormat=" + Constants.CurrentMapFormat);
				stream.WriteLine("Name=" + name);
				stream.WriteLine("Size=" + size);

				var terrain = string.Join(",", Enumerable.Repeat("0", size.X * size.Y));
				stream.WriteLine("Terrain=" + terrain);
			}

			// Load piece into cache, overwrite the old if there is one.
			return PieceManager.LoadPiece(new PackageFile(PackageManager.Core, name), filepath);
		}

		public static void SaveWorld(World world, string directory, string name, bool gameSave = false)
		{
			using var writer = new FileStream(directory + name + ".yaml", FileMode.OpenOrCreate);
			SaveWorld(world, writer, name, gameSave);
		}

		public static void SaveWorld(World world, Stream stream, string name, bool gameSave = false)
		{
			var saver = new TextNodeSaver();
			saver.Add("MapFormat", Constants.CurrentMapFormat);
			saver.Add("Name", name);
			saver.Add("Size", world.Map.Bounds);

			saver.Append(world.Save(gameSave));

			using var writer = new StreamWriter(stream);
			foreach (var savedString in saver.GetStrings())
				writer.WriteLine(savedString);
		}
	}
}
