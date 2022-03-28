using System.Collections.Generic;
using WarriorsSnuggery.Objects.Actors;
using WarriorsSnuggery.Objects.Weapons.Projectiles;
using WarriorsSnuggery.Physics;

namespace WarriorsSnuggery.Objects.Weapons
{
	class SplashWeapon : Weapon
	{
		readonly SplashProjectile projectile;
		readonly PhysicsRay ray;

		[Save("Repetition")]
		int repetition;
		[Save("RepetitionDelay")]
		int repetitionDelay;

		[Save("Speed")]
		readonly CPos speed;

		public SplashWeapon(World world, WeaponType type, Target target, Actor origin, uint id) : base(world, type, target, origin, id)
		{
			projectile = (SplashProjectile)type.Projectile;

			Angle = (Position - TargetPosition).FlatAngle;
			speed = calculateSpeed();

			repetition = projectile.Repetitions;
			repetitionDelay = projectile.RepetitionDelay;

			ray = new PhysicsRay(world);
		}

		public SplashWeapon(World world, WeaponInit init) : base(world, init)
		{
			projectile = (SplashProjectile)Type.Projectile;

			Angle = (Position - TargetPosition).FlatAngle;
			speed = init.Convert("Speed", CPos.Zero);

			repetition = init.Convert("Repetition", 0);
			repetitionDelay = init.Convert("RepetitionDelay", 0);

			ray = new PhysicsRay(world);
		}

		CPos calculateSpeed()
		{
			var dist = projectile.DistanceBasedOnTarget ? (int)(Position - TargetPosition).FlatDist/(projectile.RepetitionDelay * projectile.Repetitions) : projectile.DistancePerTick;
			return CPos.FromFlatAngle(Angle, dist);
		}

		public override void Tick()
		{
			base.Tick();

			if (World.Game.Editor)
				return;

			Move();

			if (repetitionDelay-- <= 0)
			{
				repetitionDelay = projectile.RepetitionDelay;
				var random = CPos.FromFlatAngle(WarriorsSnuggery.Angle.RandomAngle(World.Game.SharedRandom), projectile.Inaccuracy);
				Detonate(new Target(Position + random, Height), repetition-- == 0);
			}

			if (projectile.TrailParticles != null)
				World.Add(projectile.TrailParticles.Create(World, Position, Height));
		}

		public void Move()
		{
			var beforePos = Position;
			var beforeHeight = Height;

			DistanceTravelled += (int)speed.FlatDist;

			Position += speed;
			Height += speed.Z;

			if (Height == 0 && speed.Z < 0 || !World.IsInWorld(Position))
				Detonate(new Target(Position, 0));

			if (projectile.IgnoreCollisions)
				return;

			ray.Start = beforePos;
			ray.StartHeight = beforeHeight;
			ray.Target = Position;
			ray.TargetHeight = Height;
			ray.CalculateEnd(new[] { Origin.Physics });

			if ((beforePos - ray.End).Dist < (beforePos - Position).Dist)
				Detonate(new Target(ray.End, ray.EndHeight));
		}

		public override List<string> Save()
		{
			var list = base.Save();
			list.AddRange(SaveAttribute.GetFields(this, false));

			return list;
		}
	}
}
