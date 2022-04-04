using System.Collections.Generic;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Maps.Pieces
{
	public static class PieceManager
	{
		public static readonly Dictionary<string, Piece> Pieces = new Dictionary<string, Piece>();

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
				LoadPiece(new PackageFile(package, name), filepath);
			}
		}

		public static Piece LoadPiece(PackageFile packageFile, string filepath)
		{
			// Remove piece if already loaded
			var existingPiece = getPieceSoft(filepath);
			if (existingPiece != null)
				Pieces.Remove(filepath);

			var piece = new Piece(packageFile, filepath);

			Pieces.Add(filepath, piece);

			return piece;
		}

		public static Piece ReloadPiece(PackageFile packageFile)
		{
			var filepath = FileExplorer.FindIn(packageFile.Package.PiecesDirectory, packageFile.File, ".yaml");

			var existingPiece = getPieceSoft(filepath);
			if (existingPiece == null)
				throw new MissingPieceException(packageFile.ToString());

			Pieces.Remove(filepath);

			var piece = new Piece(packageFile, existingPiece.Filepath);
			Pieces.Add(piece.Filepath, piece);

			return piece;
		}

		public static Piece GetPiece(PackageFile packageFile)
		{
			var filepath = FileExplorer.FindIn(packageFile.Package.PiecesDirectory, packageFile.File, ".yaml");

			var existingPiece = getPieceSoft(filepath);
			if (existingPiece == null)
				throw new MissingPieceException(packageFile.ToString());

			return existingPiece;
		}

		static Piece getPieceSoft(string filepath)
		{
			if (Pieces.ContainsKey(filepath))
				return Pieces[filepath];

			return null;
		}
	}
}
