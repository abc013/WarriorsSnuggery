using System;
using System.Collections.Generic;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Physics;

namespace WarriorsSnuggery.Objects.Weapons.Warheads
{
	public class ForceWarhead : IWarhead
	{
		[Desc("Detonation will thrust actors away from its origin by the specified acceleration.")]
		public readonly int Acceleration;

		[Desc("Detonation will thrust actors into the sky by the specified acceleration.")]
		public readonly int HeightAcceleration;

		[Desc("Damage percentage at each range step.")]
		public readonly float[] Falloff = new[] { 1f, 1f, 0.5f, 0.25f, 0.125f, 0.0f };

		[Desc("Range steps used for falloff.", "Defines at which range the falloff points are defined.")]
		public readonly int[] RangeSteps = new[] { 0, 256, 512, 1024, 2048, 3096 };

		readonly int maxRange;

		public ForceWarhead(List<TextNode> nodes)
		{
			TypeLoader.SetValues(this, nodes);

			if (RangeSteps.Length != Falloff.Length)
				throw new InvalidNodeException($"Range step length ({RangeSteps.Length}) does not match with given falloff values ({Falloff.Length}).");

			maxRange = FalloffHelper.GetMax(Falloff, RangeSteps);
		}

		public void Impact(World world, Weapon weapon, Target target)
		{
			if (Acceleration != 0)
			{
				var ray = new PhysicsRay(world);
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

						ray.Start = actor.Position;
						ray.Target = target.Position;
						var pen = ray.GetWallPenetrationValue();

						if (pen == 0f)
							continue;

						if (Acceleration != 0)
						{
							var acceleration = (int)Math.Floor(multiplier * Acceleration * weapon.DamageModifier * pen);
							if (acceleration != 0)
								actor.Push((target.Position - actor.Position).FlatAngle, acceleration);
						}

						if (HeightAcceleration != 0)
						{
							var heightAcceleration = (int)Math.Floor(multiplier * HeightAcceleration * weapon.DamageModifier * pen);
							if (heightAcceleration != 0)
								actor.Lift(heightAcceleration);
						}
					}
				}
			}
		}
	}
}
