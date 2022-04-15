using System;
using System.Collections.Generic;
using WarriorsSnuggery.Objects.Actors;
using WarriorsSnuggery.Objects.Weapons.Projectiles;
using WarriorsSnuggery.Physics;

namespace WarriorsSnuggery.Objects.Weapons
{
	class BulletWeapon : Weapon
	{
		readonly BulletProjectile projectile;
		readonly PhysicsRay ray;

		[Save("Speed")]
		Vector speed;
		[Save("SpeedLeft")]
		Vector speedLeft;

		public BulletWeapon(World world, WeaponType type, Target target, Actor origin, uint id) : base(world, type, target, origin, id)
		{
			projectile = (BulletProjectile)type.Projectile;

			Angle = (Position - TargetPosition).FlatAngle;

			TargetPosition += getInaccuracy(projectile.Inaccuracy);

			calculateStartSpeed();

			if (projectile.OrientateToTarget)
				Rotation = new VAngle(0, 0, Angle);

			ray = new PhysicsRay(world);
		}

		public BulletWeapon(World world, WeaponInit init) : base(world, init)
		{
			projectile = (BulletProjectile)Type.Projectile;

			speed = init.Convert("Speed", Vector.Zero);
			speedLeft = init.Convert("SpeedLeft", Vector.Zero);
			if (speed == Vector.Zero)
				calculateStartSpeed();

			if (projectile.OrientateToTarget)
				Rotation = new VAngle(0, 0, Angle);

			ray = new PhysicsRay(world);
		}

		void calculateStartSpeed()
		{
			var flatSpeed = Vector.FromFlatAngle(Angle, projectile.Speed);

			var zDiff = TargetHeight - Height;
			var dDiff = (int)(Position - TargetPosition).FlatDist;
			if (dDiff > Type.MaxRange * RangeModifier)
				dDiff = (int)(Type.MaxRange * RangeModifier);

			var angle2 = new CPos(-dDiff, -zDiff, 0).FlatAngle;
			var z = MathF.Sin(angle2) * projectile.Speed;
			var plusZ = (int)((dDiff / (float)projectile.Speed) * -projectile.Force.Z / 2f);

			speed = new Vector(flatSpeed.X, flatSpeed.Y, z + plusZ);
		}

		public override void Tick()
		{
			base.Tick();

			if (World.Game.Editor)
				return;

			if (projectile.OrientateToTarget)
				Rotation = new VAngle(0, 0, -(TargetPosition - GraphicPosition).FlatAngle);

			Move();

			if (projectile.TrailParticles != null)
				World.Add(projectile.TrailParticles.Create(World, Position, Height));
		}

		public void Move()
		{
			var beforePos = Position;
			var beforeHeight = Height;

			var curSpeed = speed + speedLeft;
			var x = (int)curSpeed.X;
			var y = (int)curSpeed.Y;
			var z = (int)curSpeed.Z;
			DistanceTravelled += (int)new CPos(x, y, 0).FlatDist;

			speedLeft = new Vector(curSpeed.X - x, curSpeed.Y - y, curSpeed.Z - z);

			Position += new CPos(x, y, 0);
			Height += z;
			speed += new Vector(projectile.Force.X, projectile.Force.Y, projectile.Force.Z);

			if (Math.Abs(speed.X) > projectile.MaxSpeed)
				speed = new Vector(Math.Sign(speed.X) * projectile.MaxSpeed, speed.Y, speed.Z);
			if (Math.Abs(speed.Y) > projectile.MaxSpeed)
				speed = new Vector(speed.X, Math.Sign(speed.Y) * projectile.MaxSpeed, speed.Z);
			if (Math.Abs(speed.Z) > projectile.MaxSpeed)
				speed = new Vector(speed.X, speed.Y, Math.Sign(speed.Z) * projectile.MaxSpeed);

			if (Height == 0 && z < 0 || !World.IsInWorld(Position))
				Detonate(new Target(Position, 0));

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
