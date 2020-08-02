using WarriorsSnuggery.Physics;

namespace WarriorsSnuggery.Objects.Weapons
{
	class InstantHitWeapon : Weapon
	{
		readonly InstantHitProjectileType projectileType;

		public InstantHitWeapon(World world, WeaponType type, Target target, Actor origin, uint id) : base(world, type, target, origin, id)
		{
			projectileType = (InstantHitProjectileType)type.Projectile;

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
			projectileType = (InstantHitProjectileType)Type.Projectile;
		}

		public override void Tick()
		{
			if (World.Game.Editor)
				return;

			if (Program.SharedRandom.NextDouble() > projectileType.HitChance)
			{
				Dispose();
				return;
			}

			var physics = new RayPhysics(World)
			{
				Start = Position,
				StartHeight = Height,
				Target = Target.Position,
				TargetHeight = Target.Height
			};
			physics.CalculateEnd(ignoreActors: true);

			if ((physics.End - Position).Dist < (Position - Target.Position).Dist)
				Detonate(new Target(physics.End, physics.EndHeight));
			else if (projectileType.Splash)
				Detonate(new Target(Target.Position, Target.Height));
			else
				Detonate(Target);
		}
	}
}
