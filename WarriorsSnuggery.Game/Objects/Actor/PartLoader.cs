using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Objects.Actors.Parts;

namespace WarriorsSnuggery.Objects.Actors
{
	public class PartLoader : TextNodeInitializer
	{
		public PartLoader(ActorInit init, ActorPart part) : base(getNodes(init, part)) { }

		static List<TextNode> getNodes(ActorInit init, ActorPart part)
		{
			var type = part.GetType();
			var specification = part.Specification;

			// TODO: remove n.Key == type.Name, it is outdated from MapFormat 3.
			var parent = init.Nodes.FirstOrDefault(n => (n.Key == type.Name || n.Key == type.Name[..^4]) && (specification == null || specification == n.Specification));

			if (parent == null)
				return new List<TextNode>();

			return parent.Children;
		}
	}
}
