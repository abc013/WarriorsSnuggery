using System.Collections.Generic;
using System.IO;
using System.Linq;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Maps
{
	public static class PieceManager
	{
		public static readonly List<Piece> Pieces = new List<Piece>();

		public static void Load()
		{
			searchFiles(FileExplorer.Pieces);
		}

		static void searchFiles(string path)
		{
			var files = Directory.GetFiles(path).Where(s => s.EndsWith(".yaml"));
			foreach (var file in files)
			{
				var name = file.Remove(0, file.LastIndexOf(FileExplorer.Separator) + 1);
				name = name.Remove(name.Length - 5);

				var nodes = TextNodeLoader.FromFile(path + FileExplorer.Separator, name + ".yaml");

				Pieces.Add(new Piece(name, path, nodes));
			}

			foreach (var dir in Directory.GetDirectories(path))
				searchFiles(dir);
		}

		public static Piece ReloadPiece(string name)
		{
			var existingPiece = Pieces.FirstOrDefault(p => p.InnerName == name);

			if (existingPiece != null)
				Pieces.Remove(existingPiece);

			var path = FileExplorer.FindPath(FileExplorer.Pieces, name, ".yaml");
			var nodes = TextNodeLoader.FromFile(path, name + ".yaml");

			var piece = new Piece(name, path, nodes);
			Pieces.Add(piece);

			return piece;
		}

		public static Piece GetPiece(string piece)
		{
			var existingPiece = Pieces.FirstOrDefault(p => p.InnerName == piece);

			if (existingPiece == null)
				throw new MissingPieceException(piece);

			return existingPiece;
		}
	}
}
