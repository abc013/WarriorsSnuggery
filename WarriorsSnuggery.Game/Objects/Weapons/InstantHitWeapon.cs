using WarriorsSnuggery.Objects.Actors;
using WarriorsSnuggery.Objects.Weapons.Projectiles;
using WarriorsSnuggery.Physics;

namespace WarriorsSnuggery.Objects.Weapons
{
	class InstantHitWeapon : Weapon
	{
		readonly InstantHitProjectile projectile;

		public InstantHitWeapon(World world, WeaponType type, Target target, Actor origin, uint id) : base(world, type, target, origin, id)
		{
			projectile = (InstantHitProjectile)type.Projectile;

			var diff = Position - TargetPosition;
			if (diff.FlatDist > type.MaxRange * RangeModifier)
			{
				var angle = diff.FlatAngle;
				TargetPosition = clampToMaxRange(Position, angle);
				Target = new Target(TargetPosition, 0);
			}
		}

		public InstantHitWeapon(World world, WeaponInit init) : base(world, init)
		{
			projectile = (InstantHitProjectile)Type.Projectile;
		}

		public override void Tick()
		{
			if (World.Game.Editor)
				return;

			if (Program.SharedRandom.NextDouble() > projectile.HitChance)
			{
				Dispose();
				return;
			}

			var ray = new PhysicsRay(World)
			{
				Start = Position,
				StartHeight = Height,
				Target = Target.Position,
				TargetHeight = Target.Height
			};
			ray.CalculateEnd(ignoreActors: true, onlyToTarget: true);

			if ((ray.End - Position).Dist < (Position - Target.Position).Dist)
				Detonate(new Target(ray.End, ray.EndHeight));
			else if (projectile.Splash)
				Detonate(new Target(Target.Position, Target.Height));
			else
				Detonate(Target);
		}
	}
}
