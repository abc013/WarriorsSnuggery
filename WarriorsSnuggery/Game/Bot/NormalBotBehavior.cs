using System.Linq;
using WarriorsSnuggery.Objects.Weapons;

namespace WarriorsSnuggery.Objects.Bot
{
	public class NormalBotBehavior : BotBehavior
	{
		int moral
		{
			get
			{
				return moralVal;
			}
			set
			{
				moralVal += value;
				if (moralVal < -50)
					moralVal = -50;
				if (moralVal > 50)
					moralVal = 50;
			}
		}
		int moralVal;

		public NormalBotBehavior(World world, Actor self) : base(world, self) { }

		public override void Tick()
		{
			if (!CanMove && !CanAttack)
				return;

			if (!perfectTarget())
			{
				moral--;
				searchTarget();
				if (CanMove && Target != null)
				{
					if (DistToTarget > 712)
						Self.Accelerate(AngleToTarget);
				}

				return;
			}

			// Look if we have a weapon and are in weapon range
			if (CanAttack && Target.Actor != null)
			{
				Self.ActiveWeapon.Target = Target.Position;
				int range = Self.ActiveWeapon.Type.MaxRange;
				range /= moral > 40 ? 2 : 1;

				if (DistToTarget < range)
				{
					Self.Attack(Target.Actor);
					moral++;
				}
				else if (!CanMove)
				{
					searchTarget();
				}
			}

			if (CanMove)
			{
				var range = 5120;
				if (CanAttack)
					range = Self.ActiveWeapon.Type.MaxRange;
				else if (Self.RevealsShroudPart != null)
					range = Self.RevealsShroudPart.Range * 512;

				if (moral >= 0)
				{
					if (DistToTarget > range * 0.9f)
						Self.Accelerate(AngleToTarget);
					else if (DistToTarget < range * 0.8f)
						Self.Accelerate(-AngleToTarget);
				}
				else
				{
					if (DistToTarget > range * 1.1f)
						Self.Accelerate(AngleToTarget);
					else if (DistToTarget < range * 0.9f)
						Self.Accelerate(-AngleToTarget);
				}
			}
		}

		public override void OnDamage(Actor damager, int damage)
		{
			moral -= damage * 2;

			if (damager == null || damager.Health == null)
				return;

			if (Target == null || Target.Actor == null)
				Target = new Target(damager.Position, damager.Height);
		}

		public override void OnKill(Actor killer)
		{
			moral += 10;
		}

		bool perfectTarget()
		{
			return Target != null && Target.Actor != null && Target.Actor.IsAlive && !Target.Actor.Disposed;
		}

		void searchTarget()
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

				checkTarget(actor);
			}
		}

		void checkTarget(Actor actor)
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
