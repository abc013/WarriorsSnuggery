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
				Target = new Target(TargetPosition);
			}
		}

		public InstantHitWeapon(World world, WeaponInit init) : base(world, init)
		{
			projectile = (InstantHitProjectile)Type.Projectile;

			Load(init);
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
				Target = Target.Position,
			};
			ray.CalculateEnd(ignoreActors: true, onlyToTarget: true);

			if ((ray.End - Position).SquaredFlatDist < (Position - Target.Position).SquaredFlatDist)
				Detonate(new Target(ray.End));
			else if (projectile.Splash)
				Detonate(new Target(Target.Position));
			else
				Detonate(Target);
		}
	}
}
