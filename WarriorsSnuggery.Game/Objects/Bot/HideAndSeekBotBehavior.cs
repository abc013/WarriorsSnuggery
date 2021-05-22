using WarriorsSnuggery.Objects.Weapons;

namespace WarriorsSnuggery.Objects.Bot
{
	public class HideAndSeekBotBehavior : BotBehavior
	{
		bool hide;
		int hideDuration;

		public HideAndSeekBotBehavior(World world, Actor self) : base(world, self) { }

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

			if (hide)
			{
				if (hideDuration-- <= 0 && (Self.Weapon == null || Self.Weapon.ReloadDone))
					hide = false;
			}
			else
			{
				// Look if we have a weapon and are in weapon range
				if (CanAttack && Target.Actor != null)
				{
					Self.Weapon.Target = Target.Position;
					int range = Self.Weapon.Type.MaxRange;

					if (DistToTarget < range * 1.1f)
					{
						PredictiveAttack(Target);
						hide = true;
					}
					else if (!CanMove)
						Target = null; // Discard target if out of range
				}
			}

			if (CanMove)
			{
				var range = 5120;
				if (CanAttack)
					range = Self.Weapon.Type.MaxRange;
				else if (Self.RevealsShroud != null)
					range = Self.RevealsShroud.Range * 512;
				if (hide)
					range *= 10;

				var actor = GetNeighborActor();
				float angle = actor != null ? (Self.Position - actor.Position).FlatAngle : AngleToTarget;

				if (DistToTarget > range * 0.9f)
					Self.AccelerateSelf(angle);
				else if (DistToTarget < range * 0.8f)
					Self.AccelerateSelf(-angle);
			}
		}

		public override void OnDamage(Actor damager, int damage)
		{
			base.OnDamage(damager, damage);

			hideDuration += damage;
			hide = true;
		}

		public override void OnKill(Actor killer)
		{
			hideDuration -= 10;
		}
	}
}

