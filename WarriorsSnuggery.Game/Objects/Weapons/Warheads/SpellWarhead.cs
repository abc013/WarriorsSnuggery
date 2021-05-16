using System.Collections.Generic;
using WarriorsSnuggery.Physics;
using WarriorsSnuggery.Spells;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Objects.Weapons.Warheads
{
	public class SpellWarhead : IWarhead
	{
		[Desc("Spell to be casted on the actors.")]
		public readonly Spell Spell;

		[Desc("Ignore walls that are inbetween detonation origin and actor.")]
		public readonly bool IgnoreWalls;

		[Desc("Probability of casting that spell on the actor.")]
		public readonly float Probability = 1f;

		[Desc("Damage percentage at each range step.")]
		public readonly float[] ProbabilityFalloff = new[] { 1f, 1f, 0.5f, 0.25f, 0.125f, 0.0f };

		[Desc("Range steps used for falloff.", "Defines at which range the falloff points are defined.")]
		public readonly int[] RangeSteps = new[] { 0, 256, 512, 1024, 2048, 3096 };

		readonly int maxRange;

		public SpellWarhead(List<TextNode> nodes)
		{
			TypeLoader.SetValues(this, nodes);

			if (RangeSteps.Length != ProbabilityFalloff.Length)
				throw new InvalidNodeException($"Range step length ({RangeSteps.Length}) does not match with given falloff values ({ProbabilityFalloff.Length}).");

			maxRange = FalloffHelper.GetMax(ProbabilityFalloff, RangeSteps);
		}

		public void Impact(World world, Weapon weapon, Target target)
		{
			if (Probability != 0 && Spell != null)
			{
				if (target.Type == TargetType.ACTOR)
				{
					target.Actor.CastSpell(Spell);
					return;
				}

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

						var probability = Probability * FalloffHelper.GetMultiplier(ProbabilityFalloff, RangeSteps, dist, weapon.DamageRangeModifier);

						if (!IgnoreWalls)
						{
							physics.Start = actor.Position;
							physics.Target = target.Position;
							var pen = physics.GetWallPenetrationValue();

							if (pen == 0f)
								continue;

							probability *= Probability;
						}

						if (probability == 0 || Program.SharedRandom.NextDouble() > probability)
							continue;

						actor.CastSpell(Spell);
					}
				}
			}
		}
	}
}
