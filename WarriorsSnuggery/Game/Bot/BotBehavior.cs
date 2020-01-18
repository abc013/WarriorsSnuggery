using WarriorsSnuggery.Objects.Weapons;

namespace WarriorsSnuggery.Objects.Bot
{
	public abstract class BotBehavior
	{
		protected const int SearchIntervall = 20;

		public Target Target;
		protected float TargetFavor;

		protected readonly World World;
		protected readonly Actor Self;

		protected float DistToTarget
		{
			get { return (Target.Position - Self.Position).FlatDist; }
			set { }
		}
		protected float AngleToTarget
		{
			get { return (Self.Position - Target.Position).FlatAngle; }
			set { }
		}

		protected bool CanMove
		{
			get { return Self.Mobility != null; }
			set { }
		}
		protected bool CanAttack
		{
			get { return Self.ActiveWeapon != null; }
			set { }
		}

		protected BotBehavior(World world, Actor self)
		{
			World = world;
			Self = self;
		}

		public abstract void Tick();

		public abstract void OnDamage(Actor damager, int damage);

		public abstract void OnKill(Actor killer);

		public void GiveTarget(Target target)
		{
			Target = target;
		}
	}
}
