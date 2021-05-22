using WarriorsSnuggery.Objects.Actors;
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
					Self.AccelerateSelf(AngleToTarget);

				return;
			}

			if (CanAttack)
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
				if (DistToMapEdge > 1536)
				{
					var range = 5120;
					if (CanAttack)
						range = Self.Weapon.Type.MaxRange;
					else if (Self.RevealsShroud != null)
						range = Self.RevealsShroud.Range * 512;

					var actor = GetNeighborActor();
					float angle = actor != null ? (Self.Position - actor.Position).FlatAngle : AngleToTarget;

					if (DistToTarget > range * 0.9f)
						Self.AccelerateSelf(angle);
					else if (DistToTarget < range * 0.8f)
						Self.AccelerateSelf(-angle);
				}
				else
				{
					Self.AccelerateSelf(AngleToMapMid);
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
