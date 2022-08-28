using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WarriorsSnuggery.Loader
{
	public static class TextNodeLoader
	{
		public static List<TextNode> FromFilepath(string filepath)
		{
			return FromFile(FileExplorer.FileDirectory(filepath), FileExplorer.FileName(filepath) + FileExplorer.FileExtension(filepath));
		}

		public static List<TextNode> FromArray(byte[] data, string origin)
		{
			return fromLines(Networking.NetworkUtils.ToString(data).Split('\n'), origin);
		}

		public static List<TextNode> FromFile(string directory, string file)
		{
			return fromLines(File.ReadAllLines(directory + file), file);
		}

		static List<TextNode> fromLines(string[] lines, string file)
		{
			var list = new List<TextNode>();

			TextNode before = null;
			for (int lineNumber = 0; lineNumber < lines.Length; lineNumber++)
			{
				var line = lines[lineNumber];

				if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
					continue;

				var now = nodeFromLine($"{file}:{lineNumber + 1}", line, before);
				if (now.Parent == null)
					list.Add(now);

				before = now;
			}

			return list;
		}

		static TextNode nodeFromLine(string origin, string line, TextNode before)
		{
			var @order = (short)line.Count(c => c == '\t');
			var strings = line.Split('=', 2, StringSplitOptions.TrimEntries);

			if (strings.Length < 2)
				throw new InvalidNodeException($"[{origin}] Missing '=' in '{line}'.");

			var keyParts = strings[0].Split('@', 2, StringSplitOptions.TrimEntries);

			var node = new TextNode(origin, @order, keyParts[0], keyParts.Length > 1 ? keyParts[1] : null, strings[1]);

			if (before == null)
			{
				if (@order > 0)
					throw new InvalidNodeException($"[{origin}] '{line}' has invalid intendation at beginning of file: {@order}.");

				return node;
			}

			if (@order - before.Order == 1)
			{
				node.Parent = before;
				before.Children.Add(node);

				return node;
			}

			if (@order - before.Order <= 0)
			{
				var parent = before;
				for (var i = @order - before.Order; i <= 0; i++)
					parent = parent.Parent;

				if (parent == null)
					return node;

				node.Parent = parent;
				parent.Children.Add(node);

				return node;
			}

			throw new InvalidNodeException($"[{origin}] '{line}' has invalid intendation (difference: {before.Order - @order}).");
		}
	}
}
