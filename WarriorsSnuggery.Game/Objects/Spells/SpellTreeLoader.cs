using System.Collections.Generic;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Spells
{
	public static class SpellTreeLoader
	{
		public static readonly List<SpellTreeNode> SpellTree = new List<SpellTreeNode>();

		public static void Load(List<TextNode> nodes)
		{
			foreach (var node in nodes)
				SpellTree.Add(new SpellTreeNode(node.Children, node.Key));
		}
	}
}
