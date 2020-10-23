﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WarriorsSnuggery.Maps
{
	public static class PieceManager
	{
		public static readonly List<Piece> Pieces = new List<Piece>();

		public static void RefreshPieces()
		{
			searchFiles(FileExplorer.Maps, false);
		}

		static void searchFiles(string path, bool catchFilesInDirectory = true)
		{
			foreach (var dir in Directory.GetDirectories(path))
				searchFiles(dir);

			if (catchFilesInDirectory)
			{
				var files = Directory.GetFiles(path).Where(s => s.EndsWith(".yaml", StringComparison.CurrentCulture));
				foreach (var file in files)
				{
					var name = file.Remove(0, file.LastIndexOf('\\') + 1);
					name = name.Remove(name.Length - 5);

					var nodes = RuleReader.Read(path + @"\", name + ".yaml");

					Pieces.Add(new Piece(name, path, nodes));
				}
			}
		}

		public static void RefreshPiece(string piece)
		{
			var existingPiece = Pieces.FirstOrDefault(p => p.InnerName == piece);

			if (existingPiece != null)
				Pieces.Remove(existingPiece);

			var path = FileExplorer.FindPath(FileExplorer.Maps, piece, ".yaml");
			var nodes = RuleReader.Read(path, piece + ".yaml");

			Pieces.Add(new Piece(piece, path, nodes));
		}

		public static Piece GetPiece(string piece)
		{
			var existingPiece = Pieces.FirstOrDefault(p => p.InnerName == piece);

			if (existingPiece == null)
				throw new TextNodeException(piece);

			return existingPiece;
		}
	}
}
