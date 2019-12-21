using System;

namespace WarriorsSnuggery.Objects.Weapons
{
	public class DamageWarhead : IWarhead
	{
		[Desc("Damage to be dealt.")]
		public readonly int Damage;

		[Desc("Determines whether damage affects only walls.")]
		public readonly bool AgainstWalls;

		[Desc("Falloff if weapon is not aimed against a single actor.", "possible: QUADRATIC, CUBIC, EXPONENTIAL, LINEAR, ROOT;")]
		public readonly FalloffType Falloff = FalloffType.QUADRATIC;

		public DamageWarhead(MiniTextNode[] nodes)
		{
			Loader.PartLoader.SetValues(this, nodes);
		}

		public void Impact(World world, Weapon weapon, Target target)
		{
			if (Damage != 0)
			{
				if (target.Type == TargetType.ACTOR && !AgainstWalls)
				{
					if (weapon.Origin != null)
						target.Actor.Damage(weapon.Origin, Damage);
					else
						target.Actor.Damage(Damage);
					return;
				}

				if (!AgainstWalls)
				{
					foreach (var actor in world.Actors)
					{
						if (!actor.IsAlive || actor.Health == null)
							continue;

						var dist = (target.Position - actor.Position).FlatDist / 512;
						if (dist > 32f) continue;
						if (dist < 1f) dist = 1;

						float damagemultiplier = getDamageMultiplier(dist);
						var damage = (int)Math.Floor(damagemultiplier * Damage * weapon.DamageModifier);

						if (damage == 0)
							continue;

						if (actor.WorldPart != null && actor.WorldPart.ShowDamage)
							world.Add(new ActionText(actor.Position + new CPos(0, 0, 1024), new CPos(0, -15, 30), 50, ActionText.ActionTextType.SCALE, new Color(1f, 1 - (damage / (Damage * 1.5f)), 0).ToString() + damage.ToString()));

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

						var dist = (target.Position - wall.Position).FlatDist / 512;
						if (dist > 32f) continue;
						if (dist < 1f) dist = 1;

						float damagemultiplier = getDamageMultiplier(dist);
						var damage = (int)Math.Floor(damagemultiplier * Damage * weapon.DamageModifier);

						if (damage == 0)
							continue;

						wall.Damage(damage);
					}
				}
			}
		}

		float getDamageMultiplier(float dist)
		{
			switch (Falloff)
			{
				case FalloffType.LINEAR:
					return 1 / dist;
				case FalloffType.QUADRATIC:
					return 1 / (dist * dist);
				case FalloffType.CUBIC:
					return 1 / (dist * dist * dist);
				case FalloffType.EXPONENTIAL:
					return 1 / (float)Math.Pow(2, dist);
				case FalloffType.ROOT:
					return 1 / (float)Math.Sqrt(dist);
				default:
					return 1;
			}
		}
	}
}
