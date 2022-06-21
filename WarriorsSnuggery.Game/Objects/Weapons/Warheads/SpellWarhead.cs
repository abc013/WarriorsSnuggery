using System.Collections.Generic;
using WarriorsSnuggery.Physics;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Spells;
using WarriorsSnuggery.Objects.Actors.Parts;

namespace WarriorsSnuggery.Objects.Weapons.Warheads
{
	public class SpellWarhead : IWarhead
	{
		[Require, Desc("Effect to be casted on the actors.")]
		public readonly Effect Effect;

		[Desc("Ignore walls that are inbetween detonation origin and actor.")]
		public readonly bool IgnoreWalls;

		[Desc("Probability of casting that spell on the actor.")]
		public readonly float Probability = 1f;

		[Desc("Damage percentage at each range step.")]
		public readonly float[] ProbabilityFalloff = new[] { 1f, 1f, 0.5f, 0.25f, 0.125f, 0.0f };

		[Desc("Range steps used for falloff.", "Defines at which range the falloff points are defined.")]
		public readonly int[] RangeSteps = new[] { 0, 256, 512, 1024, 2048, 3096 };

		[Desc("Probabilites for each armor.", "The higher the value, the more likely it is for the spell to be casted. This value will be multiplied on top of the others.")]
		public readonly Dictionary<string, float> ArmorModifiers = new Dictionary<string, float>();

		readonly int maxRange;

		public SpellWarhead(List<TextNode> nodes)
		{
			var fields = TypeLoader.GetFields(this);

			foreach (var node in nodes)
			{
				switch (node.Key)
				{
					case nameof(ArmorModifiers):
						foreach (var node2 in node.Children)
							ArmorModifiers.Add(node2.Key, node2.Convert<float>());

						break;
					default:
						TypeLoader.SetValue(this, fields, node);
						break;
				}
			}

			if (RangeSteps.Length != ProbabilityFalloff.Length)
				throw new InvalidNodeException($"Range step length ({RangeSteps.Length}) does not match with given falloff values ({ProbabilityFalloff.Length}).");

			maxRange = FalloffHelper.GetMax(ProbabilityFalloff, RangeSteps);
		}

		public void Impact(World world, Weapon weapon, Target target)
		{
			if (Probability != 0 && Effect != null)
			{
				if (target.Type == TargetType.ACTOR)
				{
					target.Actor.CastEffect(Effect);
					return;
				}

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

						var probability = Probability * FalloffHelper.GetMultiplier(ProbabilityFalloff, RangeSteps, dist, weapon.DamageRangeModifier);

						if (!IgnoreWalls)
						{
							ray.Start = actor.Position;
							ray.Target = target.Position;
							var pen = ray.GetWallPenetrationValue();

							if (pen == 0f)
								continue;

							probability *= Probability;
						}

						var armor = actor.GetPartOrDefault<ArmorPart>();

						if (armor != null && ArmorModifiers.ContainsKey(armor.Name))
							probability *= Probability * ArmorModifiers[armor.Name];

						if (probability == 0 || Program.SharedRandom.NextDouble() > probability)
							continue;

						actor.CastEffect(Effect);
					}
				}
			}
		}
	}
}
