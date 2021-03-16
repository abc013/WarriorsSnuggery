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
			if (Self.World.Game.Editor)
				return;

			if (!CanMove && !CanAttack)
				return;

			if (!PerfectTarget())
			{
				SearchTarget();
				if (CanMove && Target != null && DistToTarget > 712)
					Self.Accelerate(AngleToTarget);

				return;
			}

			// Look if we have a weapon and are in weapon range
			if (CanAttack && Target.Actor != null)
			{
				Self.ActiveWeapon.Target = Target.Position;
				int range = Self.ActiveWeapon.Type.MaxRange;
				if (DistToTarget < range * 1.1f)
					Self.PrepareAttack(Target);
				else if (!CanMove)
					Target = null; // Discard target if out of range
			}

			if (CanMove)
			{
				var range = 5120;
				if (CanAttack)
					range = Self.ActiveWeapon.Type.MaxRange;
				else if (Self.RevealsShroudPart != null)
					range = Self.RevealsShroudPart.Range * 512;

				var angle = -AngleToNearActor;
				if (float.IsInfinity(angle))
					angle = AngleToTarget;

				if (DistToMapEdge > 1536)
				{
					if (moral >= 0)
					{
						if (DistToTarget > range * 0.8f)
							Self.Accelerate(angle);
						else if (DistToTarget < range * 0.7f)
							Self.Accelerate(-angle);
					}
					else
					{
						if (DistToTarget > range)
							Self.Accelerate(angle);
						else if (DistToTarget < range * 0.9f)
							Self.Accelerate(-angle);
					}
				}
				else
				{
					Self.Accelerate(AngleToMapMid);
				}
			}

			moral++;
		}

		public override void OnDamage(Actor damager, int damage)
		{
			moral -= damage;

			if (damager == null || damager.Health == null)
				return;

			if (!PerfectTarget())
				Target = new Target(damager.Position, damager.Height);
		}

		public override void OnKill(Actor killed)
		{
			base.OnKill(killed);
			moral += 10;
		}
	}
}
