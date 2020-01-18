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
				range /= moral > 40 ? 2 : 1;

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

			moral++;
		}

		public override void OnDamage(Actor damager, int damage)
		{
			moral -= damage;

			if (damager == null || damager.Health == null)
				return;

			if (Target == null || Target.Actor == null)
				Target = new Target(damager.Position, damager.Height);
		}

		public override void OnKill(Actor killer)
		{
			moral += 10;
		}
	}
}
