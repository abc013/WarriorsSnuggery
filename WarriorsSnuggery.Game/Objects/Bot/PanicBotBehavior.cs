using WarriorsSnuggery.Objects.Weapons;

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

				if (CanMove && DistToTarget > 512)
					Self.Accelerate(angle);

				if (!PerfectTarget() && Program.SharedRandom.Next(100) == 0)
					PredictiveAttack(new Target(randomPosition(), 0));
				else
					PredictiveAttack(new Target((Target.Position + randomPosition()) / new CPos(2, 2, 2), Target.Height));
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
							range = Self.ActiveWeapon.Type.MaxRange;
						else if (Self.RevealsShroudPart != null)
							range = Self.RevealsShroudPart.Range * 512;

						var actor = GetNeighborActor();
						float angle = actor != null ? (Self.Position - actor.Position).FlatAngle : AngleToTarget;

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
		}

		CPos randomPosition()
		{
			var x = Program.SharedRandom.Next(5120);
			var y = Program.SharedRandom.Next(5120);

			return Self.Position + new CPos(x, y, 0);
		}

		public override void OnDamage(Actor damager, int damage)
		{
			base.OnDamage(damager, damage);

			panic += damage * 2;
		}

		public override void OnKill(Actor killer)
		{
			panic = 0;
		}
	}
}
