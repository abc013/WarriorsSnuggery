using System;
using System.Collections.Generic;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Objects.Actors.Parts
{
	public sealed class PartInitSet
	{
		public readonly string InternalName;
		public readonly List<TextNode> Nodes;

		public PartInitSet(string internalName, List<TextNode> nodes)
		{
			InternalName = internalName;
			Nodes = nodes;
		}
	}

	public abstract class PartInfo
	{
		public readonly string InternalName;

		public ActorPart Create(Actor self)
		{
			var infoType = this.GetType();
			var type = Type.GetType(infoType.FullName[0..^4], true, true);

			return (ActorPart)Activator.CreateInstance(type, new object[] { self, this });
		}

		protected PartInfo(PartInitSet set)
		{
			InternalName = set.InternalName;
			TypeLoader.SetValues(this, set.Nodes);
		}
	}

	public abstract class ActorPart
	{
		protected readonly Actor self;

		protected ActorPart(Actor self)
		{
			this.self = self;
		}
	}
}
