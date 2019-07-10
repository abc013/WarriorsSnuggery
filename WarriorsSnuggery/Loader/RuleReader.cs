using System;
using System.Collections.Generic;
using System.IO;

namespace WarriorsSnuggery
{
	public static class RuleReader
	{
		public static List<MiniTextNode> Read(string directory, string file)
		{
			List<MiniTextNode> list;
			List<string> filesToInclude;

			using (var input = new StreamReader(directory + file, true))
			{
				Loop(file, input, out list, out filesToInclude);
				input.Close();
			}
			// Read included files as well and add them to the list
			foreach (var fileToInclude in filesToInclude)
			{
				list.AddRange(Read(directory, fileToInclude));
			}

			return list;
		}

		static void Loop(string file, StreamReader input, out List<MiniTextNode> list, out List<string> filesToInclude)
		{
			var startOfFile = true;
			MiniTextNode before = null;

			list = new List<MiniTextNode>();
			filesToInclude = new List<string>();
			while (!input.EndOfStream)
			{
				var @in = input.ReadLine();

				if (@in.Trim().Equals(string.Empty) || @in.StartsWith("#", StringComparison.CurrentCulture))
					continue;

				if (startOfFile && @in.StartsWith("@INCLUDE"))
				{
					filesToInclude.Add(@in.Remove(0, 8).Trim());
					continue;
				}
				else
				{
					startOfFile = false;
				}

				var now = ReadLine(file, @in, before);
				if (now.Parent == null)
					list.Add(now);

				before = now;
			}
		}

		static MiniTextNode ReadLine(string file, string line, MiniTextNode before)
		{
			var @order = (short)line.LastIndexOf("\t", StringComparison.CurrentCulture);
			var strings = line.Split('=');

			if (strings.Length != 2)
				throw new YamlInvalidRuleExeption(line);

			var key = strings[0].Trim();
			var value = strings[1].Trim();
			var yamlnode = new MiniTextNode(file, @order, key, value);

			if (before == null)
			{
				if (@order >= 0)
					throw new YamlInvalidRuleExeption(line);

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
				for (short i = (short)(@order - before.Order); i <= 0; i++)
				{
					parent = parent.Parent;
				}
				if (parent == null)
					return yamlnode;

				yamlnode.Parent = parent;
				parent.Children.Add(yamlnode);

				return yamlnode;
			}

			throw new YamlInvalidRuleExeption(line, -@order + before.Order);
		}
	}
}
