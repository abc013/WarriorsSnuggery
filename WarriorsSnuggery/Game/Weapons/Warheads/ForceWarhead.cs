using System;
using WarriorsSnuggery.Physics;

namespace WarriorsSnuggery.Objects.Weapons
{
	public class ForceWarhead : IWarhead
	{
		[Desc("Maximum acceleration.")]
		public readonly int Acceleration;

		[Desc("Detonation will also cause actors to fly up into the sky.")]
		public readonly bool UseHeight = true;

		[Desc("Falloff.", "possible: QUADRATIC, CUBIC, EXPONENTIAL, LINEAR, ROOT;")]
		public readonly FalloffType Falloff = FalloffType.QUADRATIC;

		readonly float maxRange;

		public ForceWarhead(MiniTextNode[] nodes)
		{
			Loader.PartLoader.SetValues(this, nodes);

			maxRange = FalloffHelper.GetMax(Falloff, Acceleration);
		}

		public void Impact(World world, Weapon weapon, Target target)
		{
			if (Acceleration != 0)
			{
				var physics = new RayPhysics(world);
				foreach (var actor in world.Actors)
				{
					if (!actor.IsAlive || actor.Health == null || actor == weapon.Origin)
						continue;

					if (weapon.Origin != null && actor.Team == weapon.Origin.Team)
						continue;

					var dist = (target.Position - actor.Position).FlatDist / 512;
					if (weapon.DamageModifier == 1f)
						if (dist > maxRange) continue;
					else
						if (dist > FalloffHelper.GetMax(Falloff, Acceleration)) continue;
					if (dist < 1f) dist = 1;

					float multiplier = FalloffHelper.GetMultiplier(Falloff, dist);

					physics.Start = actor.Position;
					physics.Target = target.Position;
					var pen = physics.GetWallPenetrationValue();

					if (pen == 0f)
						continue;

					var acceleration = (int)Math.Floor(multiplier * Acceleration * weapon.DamageModifier * pen);

					if (acceleration == 0)
						continue;

					var angle = (target.Position - actor.Position).FlatAngle;
					actor.Accelerate(angle, true, acceleration);

					if (UseHeight)
						actor.AccelerateHeight(true, true, acceleration);
				}
			}
		}
	}
}
