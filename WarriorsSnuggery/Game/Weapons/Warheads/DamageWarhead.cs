using System;
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

		readonly float maxRange;

		public DamageWarhead(MiniTextNode[] nodes)
		{
			Loader.PartLoader.SetValues(this, nodes);

			if (RangeSteps.Length != Falloff.Length)
				throw new YamlInvalidNodeException(string.Format("Range step length ({0}) does not match with given falloff values ({1}).", RangeSteps.Length, Falloff.Length));

			maxRange = FalloffHelper.GetMax(Falloff, RangeSteps);
		}

		public void Impact(World world, Weapon weapon, Target target)
		{
			if (Damage != 0)
			{
				if (target.Type == TargetType.ACTOR && !AgainstWalls)
				{
					target.Actor.Damage(weapon.Origin, Damage);
					if (target.Actor.WorldPart != null && target.Actor.WorldPart.ShowDamage)
						world.Add(new ActionText(target.Actor.Position + new CPos(0, 0, 1024), new CPos(0, -15, 30), 50, ActionText.ActionTextType.SCALE, new Color(1f, 0.4f, 0).ToString() + Damage));
					return;
				}

				if (!AgainstWalls)
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

						float damagemultiplier = FalloffHelper.GetMultiplier(Falloff, RangeSteps, dist);

						physics.Start = actor.Position;
						physics.Target = target.Position;
						var pen = physics.GetWallPenetrationValue();

						if (pen == 0f)
							continue;

						var damage = (int)Math.Floor(damagemultiplier * Damage * weapon.DamageModifier * pen);

						if (damage == 0)
							continue;

						if (actor.WorldPart != null && actor.WorldPart.ShowDamage)
							world.Add(new ActionText(actor.Position + new CPos(0, 0, 1024), new CPos(0, -15, 30), 50, ActionText.ActionTextType.SCALE, new Color(1f, 1 - (damage / (Damage * 1.5f)), 0).ToString() + damage));

						if (weapon.Origin != null)
							actor.Damage(weapon.Origin, damage);
						else
							actor.Damage(damage);
					}
				}
				else
				{
					foreach (var wall in world.WallLayer.Walls)
					{
						if (wall == null || wall.Type.Invincible)
							continue;

						var dist = (target.Position - wall.Position).FlatDist;
						if (dist > 32f) continue;
						if (dist < 1f) dist = 1;

						float damagemultiplier = FalloffHelper.GetMultiplier(Falloff, RangeSteps, dist);
						var damage = (int)Math.Floor(damagemultiplier * Damage * weapon.DamageModifier);

						if (damage == 0)
							continue;

						wall.Damage(damage);
					}
				}
			}
		}
	}
}
