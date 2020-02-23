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

		[Desc("Damage percentage at each range step.")]
		public readonly float[] Falloff = new[] { 1f, 1f, 0.5f, 0.25f, 0.125f, 0.0f };

		[Desc("Range steps used for falloff.", "Defines at which range the falloff points are defined.")]
		public readonly int[] RangeSteps = new[] { 0, 256, 512, 1024, 2048, 3096 };

		readonly float maxRange;

		public ForceWarhead(MiniTextNode[] nodes)
		{
			Loader.PartLoader.SetValues(this, nodes);

			if (RangeSteps.Length != Falloff.Length)
				throw new YamlInvalidNodeException(string.Format("Range step length ({0}) does not match with given falloff values ({1}).", RangeSteps.Length, Falloff.Length));

			maxRange = FalloffHelper.GetMax(Falloff, RangeSteps, Acceleration);
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

					var dist = (target.Position - actor.Position).FlatDist;
					if (dist > maxRange) continue;
					if (dist < 1f) dist = 1;

					float multiplier = FalloffHelper.GetMultiplier(Falloff, RangeSteps, dist);

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
