using System.Collections.Generic;

namespace WarriorsSnuggery
{
	public static class SpellTreeLoader
	{
		public static readonly List<SpellTreeNode> SpellTree = new List<SpellTreeNode>();

		public static void Load(string path, string file)
		{
			var nodes = RuleReader.Read(path, file);

			List<SpellTreeNode> spelltree = new List<SpellTreeNode>();

			foreach (var node in nodes)
				spelltree.Add(new SpellTreeNode(node.Children.ToArray(), node.Key));

			SpellTree.AddRange(spelltree);
		}
	}
}
