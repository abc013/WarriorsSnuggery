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

		public List<TextNode> GetNodes(Type type, string specification = null)
		{
			var parent = nodes.FirstOrDefault(n => n.Key == type.Name && (specification == null || specification == n.Specification));

			if (parent == null)
				return new List<TextNode>();

			return parent.Children;
		}
	}
}
