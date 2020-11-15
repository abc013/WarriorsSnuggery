using System.Collections.Generic;
using WarriorsSnuggery.Objects.Actors;

namespace WarriorsSnuggery.Objects.Parts
{
	public abstract class PartInfo
	{
		public readonly string InternalName;

		public abstract ActorPart Create(Actor self);

		protected PartInfo(string internalName, List<MiniTextNode> nodes)
		{
			InternalName = internalName;
			Loader.PartLoader.SetValues(this, nodes);
		}
	}

	public abstract class ActorPart
	{
		protected readonly Actor self;

		protected ActorPart(Actor self)
		{
			this.self = self;
		}

		public virtual void OnLoad(List<MiniTextNode> nodes)
		{

		}

		public virtual PartSaver OnSave()
		{
			return new PartSaver(this, string.Empty);
		}
	}
}
