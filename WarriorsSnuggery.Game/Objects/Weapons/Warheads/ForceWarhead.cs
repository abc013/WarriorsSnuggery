using System;
using System.Collections.Generic;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Physics;

namespace WarriorsSnuggery.Objects.Weapons.Warheads
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

		readonly int maxRange;

		public ForceWarhead(List<TextNode> nodes)
		{
			Loader.TypeLoader.SetValues(this, nodes);

			if (RangeSteps.Length != Falloff.Length)
				throw new InvalidNodeException(string.Format("Range step length ({0}) does not match with given falloff values ({1}).", RangeSteps.Length, Falloff.Length));

			maxRange = FalloffHelper.GetMax(Falloff, RangeSteps);
		}

		public void Impact(World world, Weapon weapon, Target target)
		{
			if (Acceleration != 0)
			{
				var physics = new RayPhysics(world);
				var maxDist = maxRange * weapon.DamageRangeModifier;
				var sectors = world.ActorLayer.GetSectors(target.Position, (int)maxDist);
				foreach (var sector in sectors)
				{
					foreach (var actor in world.ActorLayer.Actors)
					{
						if (!actor.IsAlive || actor.Health == null || actor == weapon.Origin)
							continue;

						if (actor.Team == weapon.Team)
							continue;

						var dist = (target.Position - actor.Position).FlatDist;
						if (dist > maxRange * weapon.DamageRangeModifier) continue;
						if (dist < 1f) dist = 1;

						float multiplier = FalloffHelper.GetMultiplier(Falloff, RangeSteps, dist, weapon.DamageRangeModifier);

						physics.Start = actor.Position;
						physics.Target = target.Position;
						var pen = physics.GetWallPenetrationValue();

						if (pen == 0f)
							continue;

						var acceleration = (int)Math.Floor(multiplier * Acceleration * weapon.DamageModifier * pen);

						if (acceleration == 0)
							continue;

						var angle = (target.Position - actor.Position).FlatAngle;
						actor.Push(angle, acceleration);

						if (UseHeight)
							actor.Lift(acceleration);
					}
				}
			}
		}
	}
}
