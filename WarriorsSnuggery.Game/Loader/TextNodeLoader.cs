using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WarriorsSnuggery.Loader
{
	public static class TextNodeLoader
	{
		public static List<TextNode> FromFile(string directory, string file, bool useIncludes = true)
		{
			var lines = File.ReadAllLines(directory + file);

			startLoop(file, lines, out var list, out var filesToInclude);

			if (!useIncludes)
				return list;

			// Read included files as well and add them to the list
			foreach (var fileToInclude in filesToInclude)
				list.AddRange(FromFile(directory, fileToInclude));

			return list;
		}

		static void startLoop(string file, string[] lines, out List<TextNode> nodes, out List<string> toInclude)
		{
			toInclude = new List<string>();

			int offset;

			for (offset = 0; offset < lines.Length; offset++)
			{
				var line = lines[offset];

				if (isEmptyOrComment(line))
					continue;

				if (line.StartsWith("@INCLUDE "))
					toInclude.Add(line.Remove(0, 9).Trim());
				else
					break;
			}

			loop(file, lines, offset, out nodes);
		}

		static void loop(string file, string[] lines, int offset, out List<TextNode> list)
		{
			TextNode before = null;

			list = new List<TextNode>();
			for (int i = offset; i < lines.Length; i++)
			{
				var line = lines[i];

				if (isEmptyOrComment(line))
					continue;

				var now = nodeFromLine(file, line, offset, before);
				if (now.Parent == null)
					list.Add(now);

				before = now;
			}
		}
		
		static bool isEmptyOrComment(string input)
		{
			if (string.IsNullOrWhiteSpace(input) || input.StartsWith('#'))
				return true;

			return false;
		}

		static TextNode nodeFromLine(string file, string line, int lineNumber, TextNode before)
		{
			var @order = (short)line.Count(c => c == '\t');
			var strings = line.Split('=', 2);

			if (strings.Length < 2)
				throw new InvalidNodeException($"Missing '=' in '{line}'. ['{file}', line {lineNumber}]");

			var yamlnode = new TextNode(file, @order, strings[0].Trim(), strings[1].Trim());

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
