using System.Collections.Generic;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Objects.Actors;

namespace WarriorsSnuggery.Objects.Parts
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

		public abstract ActorPart Create(Actor self);

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

		public virtual void OnLoad(List<TextNode> nodes)
		{

		}

		public virtual PartSaver OnSave()
		{
			return new PartSaver(this, string.Empty);
		}
	}
}
