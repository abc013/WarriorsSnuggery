using System.Collections.Generic;
using System.Linq;

namespace WarriorsSnuggery.Maps.Pieces
{
	public static class PieceManager
	{
		public static readonly List<Piece> Pieces = new List<Piece>();

		public static void Load()
		{
			foreach (var package in PackageManager.ActivePackages)
				loadPackage(package);
		}

		static void loadPackage(Package package)
		{
			foreach (var filepath in FileExplorer.FilesIn(package.PiecesDirectory, ".yaml", true))
			{
				var name = FileExplorer.FileName(filepath);
				var path = FileExplorer.FileDirectory(filepath);

				Pieces.Add(new Piece(name, path));
			}
		}

		public static Piece ReloadPiece(string innerName)
		{
			var existingPiece = Pieces.First(p => p.InnerName == innerName);

			Pieces.Remove(existingPiece);

			var piece = new Piece(existingPiece.InnerName, existingPiece.Path);
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
