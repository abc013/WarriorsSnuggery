using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WarriorsSnuggery.Loader
{
	public static class TextNodeLoader
	{
		public static List<TextNode> FromFile(string directory, string file)
		{
			var lines = File.ReadAllLines(directory + file);

			var list = new List<TextNode>();

			TextNode before = null;
			for (int i = 0; i < lines.Length; i++)
			{
				var line = lines[i];

				if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
					continue;

				var now = nodeFromLine(file, line, i, before);
				if (now.Parent == null)
					list.Add(now);

				before = now;
			}

			return list;
		}

		static TextNode nodeFromLine(string file, string line, int lineNumber, TextNode before)
		{
			var @order = (short)line.Count(c => c == '\t');
			var strings = line.Split('=', 2);

			if (strings.Length < 2)
				throw new InvalidNodeException($"Missing '=' in '{line}'. ['{file}', line {lineNumber}]");

			var keyParts = strings[0].Split('@', 2);

			var yamlnode = new TextNode(file, @order, keyParts[0].Trim(), keyParts.Length > 1 ? keyParts[1].Trim() : null, strings[1].Trim());

			if (before == null)
			{
				if (@order > 0)
					throw new InvalidNodeException($"'{line}' has invalid intendation at beginning of file: {@order}. ['{file}', line {lineNumber}]");

				return yamlnode;
			}

			if (@order - before.Order == 1)
			{
				yamlnode.Parent = before;
				before.Children.Add(yamlnode);

				return yamlnode;
			}

			if (@order - before.Order <= 0)
			{
				var parent = before;
				for (var i = @order - before.Order; i <= 0; i++)
					parent = parent.Parent;

				if (parent == null)
					return yamlnode;

				yamlnode.Parent = parent;
				parent.Children.Add(yamlnode);

				return yamlnode;
			}

			throw new InvalidNodeException($"'{line}' has invalid intendation (difference: {-@order + before.Order}). ['{file}', line {lineNumber}]");
		}
	}
}
