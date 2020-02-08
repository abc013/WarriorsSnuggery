using System;
using WarriorsSnuggery.Objects.Weapons;

namespace WarriorsSnuggery.Objects.Bot
{
	public class MothBotBehavior : BotBehavior
	{
		int currentCycle;
		float mothAngle;

		public MothBotBehavior(World world, Actor self) : base(world, self) { }

		public override void Tick()
		{
			if (!CanMove && !CanAttack)
				return;

			currentCycle++;
			if (Target != null)
				mothAngle = AngleToTarget + (float)Math.PI / 2;
			else if (currentCycle % 5 == 0)
				mothAngle = Program.SharedRandom.Next();

			var curAngle = mothAngle + (float)Math.Sin(currentCycle / 30f) / 5f;

			if (!PerfectTarget())
			{
				SearchTarget();

				if (CanMove && Target != null && DistToTarget > 712)
					Self.Accelerate(AngleToTarget);
				else
					Self.Accelerate(curAngle);

				return;
			}

			if (CanAttack)
			{
				Self.ActiveWeapon.Target = Target.Position;
				int range = Self.ActiveWeapon.Type.MaxRange;
				if (DistToTarget < range * 1.1f)
					Self.Attack(Target);
				else if (!CanMove)
					SearchTarget();
			}

			if (CanMove)
			{
				var range = 5120;
				if (CanAttack)
					range = Self.ActiveWeapon.Type.MaxRange;
				else if (Self.RevealsShroudPart != null)
					range = Self.RevealsShroudPart.Range * 512;

				if (DistToTarget > range * 2f)
				{
					Target = null;
					return;
				}

				if (DistToTarget > range * 0.5f)
					Self.Accelerate(AngleToTarget);
				else if (DistToTarget < range * 0.4f)
					Self.Accelerate(-AngleToTarget);
				else
					Self.Accelerate(curAngle);
			}
		}

		public override void OnDamage(Actor damager, int damage)
		{
			if (damager == null || damager.Health == null)
				return;

			if (Target == null || Target.Actor == null)
				Target = new Target(damager.Position, damager.Height);
		}

		public override void OnKill(Actor killer) { }
	}
}
