using System;
using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Objects.Parts;
using WarriorsSnuggery.Physics;

namespace WarriorsSnuggery.Objects.Weapons
{
	public class DamageWarhead : IWarhead
	{
		[Desc("Damage to be dealt.")]
		public readonly int Damage;

		[Desc("Determines whether damage affects only walls.")]
		public readonly bool AgainstWalls;

		[Desc("Damage percentage at each range step.")]
		public readonly float[] Falloff = new[] { 1f, 1f, 0.5f, 0.25f, 0.125f, 0.0f };

		[Desc("Range steps used for falloff.", "Defines at which range the falloff points are defined.")]
		public readonly int[] RangeSteps = new[] { 0, 256, 512, 1024, 2048, 3096 };

		[Desc("Modifiers for each armor.", "The value will be multiplied with the range.")]
		public readonly Dictionary<string, float> ArmorModifiers = new Dictionary<string, float>();

		readonly int maxRange;

		public DamageWarhead(MiniTextNode[] nodes)
		{
			var fields = PartLoader.GetFields(this);

			foreach (var node in nodes)
			{
				switch(node.Key)
				{
					case "ArmorModifiers":
						foreach (var node2 in node.Children)
							ArmorModifiers.Add(node2.Key, node2.Convert<float>());

						break;
					default:
						PartLoader.SetValue(this, fields, node);
						break;
				}
			}

			if (RangeSteps.Length != Falloff.Length)
				throw new InvalidTextNodeException(string.Format("Range step length ({0}) does not match with given falloff values ({1}).", RangeSteps.Length, Falloff.Length));

			maxRange = FalloffHelper.GetMax(Falloff, RangeSteps);
		}

		public void Impact(World world, Weapon weapon, Target target)
		{
			if (Damage == 0)
				return;

			var maxDist = maxRange * weapon.DamageRangeModifier;
			if (!AgainstWalls)
			{
				if (target.Type == TargetType.ACTOR)
				{
					damageActor(world, weapon, target.Actor, Damage);
					return;
				}

				var physics = new RayPhysics(world);
				var sectors = world.ActorLayer.GetSectors(target.Position, (int)maxDist);
				foreach (var sector in sectors)
				{
					foreach (var actor in sector.Actors)
					{
						if (!actor.IsAlive || actor.Health == null || actor == weapon.Origin)
							continue;

						if (actor.Team == weapon.Team)
							continue;

						var dist = (target.Position - actor.Position).FlatDist;
						if (dist > maxDist) continue;
						if (dist < 1f) dist = 1;

						float damagemultiplier = FalloffHelper.GetMultiplier(Falloff, RangeSteps, dist, weapon.DamageRangeModifier);

						physics.Start = actor.Position;
						physics.Target = target.Position;
						var pen = physics.GetWallPenetrationValue();

						if (pen == 0f)
							continue;

						var damage = (int)Math.Floor(damagemultiplier * Damage * weapon.DamageModifier * pen);

						damageActor(world, weapon, actor, damage);
					}
				}
			}
			else
			{
				foreach (var wall in world.WallLayer.GetRange(target.Position, (int)maxDist))
				{
					if (wall == null || wall.Type.Invincible)
						continue;

					var dist = (target.Position - wall.Position).FlatDist;
					if (dist > maxDist) continue;
					if (dist < 1) dist = 1;

					float damagemultiplier = FalloffHelper.GetMultiplier(Falloff, RangeSteps, dist, weapon.DamageRangeModifier);
					var damage = (int)Math.Floor(damagemultiplier * Damage * weapon.DamageModifier);

					if (damage == 0)
						continue;

					wall.Damage(damage);
				}
			}
		}

		void damageActor(World world, Weapon weapon, Actor actor, int damage)
		{
			var armor = actor.Parts.FirstOrDefault(p => p is ArmorPart);

			if (armor != null)
			{
				var armorPart = armor as ArmorPart;

				if (ArmorModifiers.ContainsKey(armorPart.Name))
					damage = (int)(damage * ArmorModifiers[armorPart.Name]);
			}

			if (damage == 0)
				return;

			if (weapon.Origin != null)
				actor.Damage(weapon.Origin, damage);
			else
				actor.Damage(damage);

			if (actor.WorldPart != null && actor.WorldPart.ShowDamage)
				world.Add(new ActionText(actor.Position + new CPos(0, 0, 1024), new CPos(0, -15, 30), 50, ActionText.ActionTextType.SCALE, new Color(1f, 0.4f, 0).ToString() + damage));
		}
	}
}
