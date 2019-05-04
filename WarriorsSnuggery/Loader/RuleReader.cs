using System;
using System.IO;
using System.Collections.Generic;

namespace WarriorsSnuggery
{
	public static class RuleReader
	{
		public static List<MiniTextNode> Read(string file) //TODO: Add @INCLUDE %file% possibility in this file that adds new files to the list as well
		{
			List<MiniTextNode> list;

			using(var input = new StreamReader(file, true))
			{
				Loop(input, out list);
				input.Close();
			}

			return list;
		}

		static void Loop(StreamReader input, out List<MiniTextNode> list)
		{
			MiniTextNode before = null;

			list = new List<MiniTextNode>();
			while(!input.EndOfStream)
			{
				var @in = input.ReadLine();

				if (@in.Trim().Equals(string.Empty) || @in.StartsWith("#", StringComparison.CurrentCulture))
					continue;

				var now = ReadLine(@in, before);
				if (now.Parent == null)
					list.Add(now);

				before = now;
			}
		}

		static MiniTextNode ReadLine(string line, MiniTextNode before)
		{
			var @order = (short) line.LastIndexOf("\t", StringComparison.CurrentCulture);
			var strings = line.Split('=');

			if (strings.Length != 2)
				throw new YamlInvalidRuleExeption(line);

			var key = strings[0].Trim();
			var value = strings[1].Trim();
			var yamlnode = new MiniTextNode(@order, key, value);

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
				for(short i = (short) (@order - before.Order); i <= 0; i++)
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
