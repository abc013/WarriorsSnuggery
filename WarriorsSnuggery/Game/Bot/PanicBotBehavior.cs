﻿using WarriorsSnuggery.Objects.Weapons;

namespace WarriorsSnuggery.Objects.Bot
{
	public class PanicBotBehavior : BotBehavior
	{
		int panic;
		bool inPanic;
		float angle;

		public PanicBotBehavior(World world, Actor self) : base(world, self) { }

		public override void Tick()
		{
			if (!CanMove && !CanAttack)
				return;

			if (Self.IsAlive && panic > Self.Health.HP * 2)
				inPanic = true;

			if (inPanic)
			{
				if (panic-- <= 0)
					inPanic = false;

				if (panic % 20 == 0)
					angle = (float)Program.SharedRandom.NextDouble();

				if (CanMove && Target != null && DistToTarget > 512)
					Self.Accelerate(angle);

				if (!PerfectTarget() && Program.SharedRandom.Next(100) == 0)
					Self.Attack(new Target(randomPosition(), 0));
				else
					Self.Attack(new Target((Target.Position + randomPosition()) / new CPos(2, 2, 2), Target.Height));
			}
			else
			{
				if (!PerfectTarget())
				{
					SearchTarget();
					if (Self.IsAlive && panic <= Self.Health.HP * 1.8f)
						panic++;

					if (CanMove && Target != null && DistToTarget > 712)
						Self.Accelerate(AngleToTarget);

					return;
				}
				panic--;

				if (CanAttack)
				{
					Self.ActiveWeapon.Target = Target.Position;
					int range = Self.ActiveWeapon.Type.MaxRange;
					if (DistToTarget < range)
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

					if (DistToTarget > range * 0.9f)
						Self.Accelerate(AngleToTarget);
					else if (DistToTarget < range * 0.8f)
						Self.Accelerate(-AngleToTarget);
				}
			}
		}

		CPos randomPosition()
		{
			var x = Program.SharedRandom.Next(5120);
			var y = Program.SharedRandom.Next(5120);

			return Self.Position + new CPos(x, y, 0);
		}

		public override void OnDamage(Actor damager, int damage)
		{
			panic += damage * 2;

			if (damager == null || damager.Health == null)
				return;

			if (Target == null || Target.Actor == null)
				Target = new Target(damager.Position, damager.Height);
		}

		public override void OnKill(Actor killer)
		{
			panic = 0;
		}
	}
}