using System;
using WarriorsSnuggery.Physics;
using WarriorsSnuggery.Spells;

namespace WarriorsSnuggery.Objects.Weapons
{
	public class SpellWarhead : IWarhead
	{
		[Desc("Spell to be casted on the actors.")]
		public readonly Spell Spell;

		[Desc("Ignore walls that are inbetween detonation origin and actor.")]
		public readonly bool IgnoreWalls;

		[Desc("Probability of casting that spell on the actor.")]
		public readonly float Probability = 1f;

		[Desc("Falloff of the probability.")]
		public readonly FalloffType ProbabilityFalloff = FalloffType.QUADRATIC;

		readonly float maxRange = 1f;

		public SpellWarhead(MiniTextNode[] nodes)
		{
			Loader.PartLoader.SetValues(this, nodes);

			maxRange = FalloffHelper.GetMax(ProbabilityFalloff, Probability);
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
				foreach (var actor in world.Actors)
				{
					if (!actor.IsAlive || actor.Health == null || actor == weapon.Origin)
						continue;

					var dist = (target.Position - actor.Position).FlatDist / 512;
					if (dist > maxRange) continue;
					if (dist < 1f) dist = 1;

					var probability = Probability * FalloffHelper.GetMultiplier(ProbabilityFalloff, dist);

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
