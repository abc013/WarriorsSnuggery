using WarriorsSnuggery.Objects.Weapons;

namespace WarriorsSnuggery.Objects.Bot
{
	public class MothBotBehavior : BotBehavior
	{
		int tick;

		public MothBotBehavior(World world, Actor self) : base(world, self) { }

		public override void Tick()
		{
			if (!CanMove && !CanAttack)
				return;

			if (!PerfectTarget())
			{
				if (tick++ % 20 == 0)
					Target = new Target(randomPosition(), 0);

				SearchTarget();

				if (CanMove && Target != null && DistToTarget > 712)
					Self.Accelerate(AngleToTarget);

				return;
			}

			if (CanAttack)
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
				if (DistToMapEdge > 1536)
				{
					var range = 5120;
					if (CanAttack)
						range = Self.ActiveWeapon.Type.MaxRange;
					else if (Self.RevealsShroudPart != null)
						range = Self.RevealsShroudPart.Range * 512;

					var angle = -AngleToNearActor;
					if (float.IsInfinity(angle))
						angle = AngleToTarget;

					if (DistToTarget > range * 0.9f)
						Self.Accelerate(angle);
					else if (DistToTarget < range * 0.8f)
						Self.Accelerate(-angle);
				}
				else
				{
					Self.Accelerate(AngleToMapMid);
				}
			}
		}

		CPos randomPosition()
		{
			var x = Program.SharedRandom.Next(2048) - 1024;
			var y = Program.SharedRandom.Next(2048) - 1024;

			return Self.Position + new CPos(x, y, 0);
		}

		public override void OnKill(Actor killer) { }
	}
}
