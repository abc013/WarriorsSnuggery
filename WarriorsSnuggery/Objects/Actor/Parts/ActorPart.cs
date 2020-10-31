using System.Collections.Generic;
using WarriorsSnuggery.Objects.Actors;
using WarriorsSnuggery.Objects.Weapons;

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

		public virtual void Tick()
		{

		}

		public virtual void Render()
		{

		}

		public virtual void OnAttack(CPos target, Weapon weapon)
		{

		}

		public virtual void OnKill(Actor killed)
		{

		}

		public virtual void OnDamage(Actor damager, int damage)
		{

		}

		public virtual void OnKilled(Actor killer)
		{

		}

		public virtual void OnMove(CPos old, CPos speed)
		{

		}

		public virtual void OnStop()
		{

		}

		public virtual void OnAccelerate(CPos acceleration)
		{

		}

		public virtual void OnAccelerate(float angle, int acceleration)
		{

		}

		public virtual void OnDispose()
		{

		}
	}
}
