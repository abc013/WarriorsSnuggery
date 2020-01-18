using System.Linq;
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

		protected bool PerfectTarget()
		{
			return Target != null && Target.Actor != null && Target.Actor.IsAlive && !Target.Actor.Disposed;
		}

		protected virtual void SearchTarget()
		{
			if (World.Game.LocalTick % SearchIntervall != 0)
				return;

			var range = Self.RevealsShroudPart == null ? 5120 : Self.RevealsShroudPart.Range * 512;

			// Find all possible targets in range
			var targets = World.Actors.FindAll(a => a.Team != Actor.NeutralTeam && a.Team != Self.Team && (a.Position - Self.Position).FlatDist <= range);

			if (!targets.Any())
				return;

			// Loop through and find the best target
			foreach (var actor in targets)
			{
				if (actor.Health == null || !actor.IsAlive)
					continue;

				CheckTarget(actor);
			}
		}

		protected virtual void CheckTarget(Actor actor)
		{
			if (actor.Team == Self.Team)
				return;

			if (Target == null || Target.Actor == null || !Target.Actor.IsAlive || !Target.Actor.Disposed)
			{
				Target = new Target(actor);
				return;
			}

			var newFavor = 0f;

			// Factor: Health. from 0 to 1
			// If target has less health, then keep attacking it
			newFavor += Target.Actor.Health.HPRelativeToMax - actor.Health.HPRelativeToMax;

			// Factor: Distance.
			// If target is closer, then keep attacking it
			newFavor += 1 - (Self.Position - actor.Position).FlatDist / DistToTarget;

			// Factor: Player. from 0 to 1
			// If target is player, then keep attacking it
			newFavor += actor.IsPlayer ? 1 : 0;

			if (newFavor > TargetFavor)
			{
				Target = new Target(actor);
				TargetFavor = newFavor;
			}
		}
	}
}
