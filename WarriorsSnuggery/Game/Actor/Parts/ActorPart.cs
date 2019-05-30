namespace WarriorsSnuggery.Objects.Parts
{
	public abstract class PartInfo
	{
		public abstract ActorPart Create(Actor self);

		protected PartInfo(MiniTextNode[] nodes) { }
	}

	public abstract class ActorPart
	{
		protected readonly Actor self;

		protected ActorPart(Actor self)
		{
			this.self = self;
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

		public virtual void OnAccelerate(float angle, int acceleration)
		{

		}

		public virtual void OnDispose()
		{

		}
	}
}
