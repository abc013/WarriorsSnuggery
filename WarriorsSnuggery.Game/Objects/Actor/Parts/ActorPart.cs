using System;
using System.Collections.Generic;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Objects.Actors.Parts
{
	public sealed class PartInitSet
	{
		public readonly string Specification;
		public readonly List<TextNode> Nodes;

		public PartInitSet(string specification, List<TextNode> nodes)
		{
			Specification = specification;
			Nodes = nodes;
		}
	}

	public abstract class PartInfo
	{
		internal readonly string Specification;

		public ActorPart Create(Actor self)
		{
			var infoType = this.GetType();
			var type = Type.GetType(infoType.FullName[0..^4], true, true);

			return (ActorPart)Activator.CreateInstance(type, new object[] { self, this });
		}

		protected PartInfo(PartInitSet set)
		{
			Specification = set.Specification;
			TypeLoader.SetValues(this, set.Nodes);
		}
	}

	public abstract class ActorPart
	{
		protected readonly Actor Self;
		internal readonly string Specification; 

		protected ActorPart(Actor self, PartInfo info)
		{
			Self = self;
			Specification = info.Specification;
		}
	}
}
