using System.Collections.Generic;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Spells
{
	public static class SpellTreeLoader
	{
		public static readonly List<SpellTreeNode> SpellTree = new List<SpellTreeNode>();

		public static void Load(string path, string file)
		{
			var nodes = TextNodeLoader.FromFile(path, file);

			var spelltree = new List<SpellTreeNode>();
			foreach (var node in nodes)
				spelltree.Add(new SpellTreeNode(node.Children, node.Key));

			SpellTree.AddRange(spelltree);
		}
	}
}
