using System;
using WarriorsSnuggery.Physics;

namespace WarriorsSnuggery.Objects.Weapons
{
	class InstantHitWeapon : Weapon
	{
		readonly InstantHitProjectileType projectileType;

		public InstantHitWeapon(World world, WeaponType type, Target target, Actor origin) : base(world, type, target, origin)
		{
			projectileType = (InstantHitProjectileType)type.Projectile;

			var diff = Position - TargetPosition;
			if (diff.FlatDist > type.MaxRange * RangeModifier)
			{
				var angle = diff.FlatAngle;
				TargetPosition = Position + new CPos((int)(Math.Cos(angle) * type.MaxRange * RangeModifier), (int)(Math.Sin(angle) * type.MaxRange * RangeModifier), 0);
				Target = new Target(TargetPosition, 0);
			}
		}

		public override void Tick()
		{
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
			physics.CalculateEnd(Origin);

			if ((physics.End - Position).Dist < (Position - Target.Position).Dist)
				Detonate(new Target(physics.End, physics.EndHeight));
			else
				Detonate(Target);
		}
	}
}
