using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WarriorsSnuggery.Maps
{
	public static class PieceManager
	{
		static List<Piece> pieces;

		public static void RefreshPieces()
		{
			pieces = searchFiles(new List<Piece>(), FileExplorer.Maps, false);
		}

		static List<Piece> searchFiles(List<Piece> list, string path, bool catchFilesInDirectory = true)
		{
			foreach (var dir in Directory.GetDirectories(path))
				searchFiles(list, dir);

			if (catchFilesInDirectory)
			{
				var files = Directory.GetFiles(path).Where(s => s.EndsWith(".yaml", StringComparison.CurrentCulture));
				foreach (var file in files)
				{
					var name = file.Remove(0, file.LastIndexOf('\\') + 1);
					name = name.Remove(name.Length - 5);

					var nodes = RuleReader.Read(path + @"\", name + ".yaml").ToArray();

					list.Add(Piece.Load(name, path, nodes));
				}
			}

			return list;
		}

		public static void RefreshPiece(string piece)
		{
			var existingPiece = pieces.FirstOrDefault(p => p.InnerName == piece);

			if (existingPiece != null)
				pieces.Remove(existingPiece);

			var path = FileExplorer.FindPath(FileExplorer.Maps, piece, ".yaml");
			var nodes = RuleReader.Read(path, piece + ".yaml").ToArray();

			pieces.Add(Piece.Load(piece, path, nodes));
		}

		public static Piece GetPiece(string piece)
		{
			var existingPiece = pieces.FirstOrDefault(p => p.InnerName == piece);

			if (existingPiece == null)
				throw new MissingPieceException(piece);

			return existingPiece;
		}

		public static Piece[] GetPieces()
		{
			return pieces.ToArray();
		}
	}
}
