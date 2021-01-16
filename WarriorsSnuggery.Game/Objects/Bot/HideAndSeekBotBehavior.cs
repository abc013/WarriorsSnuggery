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

			if (hide)
			{
				if (hideDuration-- <= 0 && !Self.WeaponReloading)
					hide = false;
			}
			else
			{
				// Look if we have a weapon and are in weapon range
				if (CanAttack && Target.Actor != null)
				{
					Self.ActiveWeapon.Target = Target.Position;
					int range = Self.ActiveWeapon.Type.MaxRange;

					if (DistToTarget < range * 1.1f)
					{
						Self.PrepareAttack(Target);
						hide = true;
					}
					else if (!CanMove)
						SearchTarget();
				}
			}

			if (CanMove)
			{
				var range = 5120;
				if (CanAttack)
					range = Self.ActiveWeapon.Type.MaxRange;
				else if (Self.RevealsShroudPart != null)
					range = Self.RevealsShroudPart.Range * 512;
				if (hide)
					range *= 10;

				var angle = -AngleToNearActor;
				if (float.IsInfinity(angle))
					angle = AngleToTarget;

				if (DistToTarget > range * 0.9f)
					Self.Accelerate(angle);
				else if (DistToTarget < range * 0.8f)
					Self.Accelerate(-angle);
			}
		}

		public override void OnDamage(Actor damager, int damage)
		{
			hideDuration += damage;
			hide = true;

			if (damager == null || damager.Health == null)
				return;

			if (Target == null || Target.Actor == null)
				Target = new Target(damager.Position, damager.Height);
		}

		public override void OnKill(Actor killer)
		{
			hideDuration -= 10;
		}
	}
}

