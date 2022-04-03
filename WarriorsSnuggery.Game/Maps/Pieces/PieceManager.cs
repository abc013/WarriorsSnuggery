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

				LoadPiece(name, path, package);
			}
		}

		public static Piece LoadPiece(string innerName, string path, Package package)
		{
			// Remove piece if already loaded
			var existingPiece = getPieceSoft(innerName, package);

			if (existingPiece != null)
				Pieces.Remove(existingPiece);

			var piece = new Piece(innerName, path, package);

			Pieces.Add(piece);

			return piece;
		}

		public static Piece ReloadPiece(string innerName, Package package)
		{
			var existingPiece = getPieceSoft(innerName, package);

			if (existingPiece == null)
				throw new MissingPieceException(innerName);

			Pieces.Remove(existingPiece);

			var piece = new Piece(existingPiece.InnerName, existingPiece.Path, existingPiece.Package);
			Pieces.Add(piece);

			return piece;
		}

		public static Piece GetPiece(string packageFile)
		{
			var existingPiece = getPieceSoft(FileExplorer.ResolveFile(packageFile), FileExplorer.ResolvePackage(packageFile));
			if (existingPiece == null)
				throw new MissingPieceException(packageFile);

			return existingPiece;
		}

		static Piece getPieceSoft(string innerName, Package package)
		{
			return Pieces.FirstOrDefault(p => p.Package == package && p.InnerName == innerName);
		}
	}
}
