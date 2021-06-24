using System;
using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Objects.Actors
{
	public class PartLoader
	{
		readonly List<TextNode> nodes;

		public PartLoader(ActorInit init)
		{
			nodes = init.Nodes;
		}

		public List<TextNode> GetNodes(Type type, string internalName = null)
		{
			var parent = nodes.FirstOrDefault(n => n.Key == type.Name && (internalName == null || internalName == n.Value));

			if (parent == null)
				return new List<TextNode>();

			return parent.Children;
		}
	}
}
