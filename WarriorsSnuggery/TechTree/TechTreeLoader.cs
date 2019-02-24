using System;
using System.Collections.Generic;

namespace WarriorsSnuggery
{
	public static class TechTreeLoader
	{
		public static readonly List<ITechTreeNode> TechTree = new List<ITechTreeNode>();

		public static void LoadTechTree(string file)
		{
			var nodes = RuleReader.Read(FileExplorer.Rules + file + ".yaml");

			List<ITechTreeNode> techtree = new List<ITechTreeNode>();

			foreach (var node in nodes)
				techtree.Add(new ITechTreeNode(node.Children.ToArray(), node.Key));

			TechTree.AddRange(techtree);
		}
	}
}
