using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Maps.Pieces
{
	public static class PieceManager
	{
		public static readonly List<Piece> Pieces = new List<Piece>();

		public static void Load()
		{
			foreach(var filepath in FileExplorer.FilesIn(FileExplorer.Pieces, ".yaml", true))
            {
				var name = FileExplorer.FileName(filepath);
				var path = FileExplorer.FileDirectory(filepath);

				var nodes = TextNodeLoader.FromFile(path, name + ".yaml");
				Pieces.Add(new Piece(name, path, nodes));
			}
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
