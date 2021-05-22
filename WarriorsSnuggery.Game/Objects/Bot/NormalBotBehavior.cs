using System;
using WarriorsSnuggery.Objects.Weapons;

namespace WarriorsSnuggery.Objects.Bot
{
	public class NormalBotBehavior : BotBehavior
	{
		int moral
		{
			get => moralVal;
			set => moralVal = Math.Clamp(value, -50, 50);
		}
		int moralVal;

		public NormalBotBehavior(World world, Actor self) : base(world, self) { }

		public override void Tick()
		{
			if (!CanMove && !CanAttack)
				return;

			if (!PerfectTarget())
			{
				SearchTarget();
				if (CanMove && Target != null && DistToTarget > 712)
					Self.AccelerateSelf(AngleToTarget);

				return;
			}

			// Look if we have a weapon and are in weapon range
			if (CanAttack && Target.Actor != null)
			{
				Self.Weapon.Target = Target.Position;
				int range = Self.Weapon.Type.MaxRange;
				if (DistToTarget < range * 1.1f)
					PredictiveAttack(Target);
				else if (!CanMove)
					Target = null; // Discard target if out of range
			}

			if (CanMove)
			{
				var range = 5120;
				if (CanAttack)
					range = Self.Weapon.Type.MaxRange;
				else if (Self.RevealsShroud != null)
					range = Self.RevealsShroud.Range * 512;

				var actor = GetNeighborActor();
				float angle = actor != null ? (Self.Position - actor.Position).FlatAngle : AngleToTarget;

				if (DistToMapEdge > 1536)
				{
					if (moral >= 0)
					{
						if (DistToTarget > range * 0.8f)
							Self.AccelerateSelf(angle);
						else if (DistToTarget < range * 0.7f)
							Self.AccelerateSelf(-angle);
					}
					else
					{
						if (DistToTarget > range)
							Self.AccelerateSelf(angle);
						else if (DistToTarget < range * 0.9f)
							Self.AccelerateSelf(-angle);
					}
				}
				else
				{
					Self.AccelerateSelf(AngleToMapMid);
				}
			}

			moral++;
		}

		public override void OnDamage(Actor damager, int damage)
		{
			base.OnDamage(damager, damage);

			moral -= damage;
		}

		public override void OnKill(Actor killed)
		{
			base.OnKill(killed);
			moral += 10;
		}
	}
}
