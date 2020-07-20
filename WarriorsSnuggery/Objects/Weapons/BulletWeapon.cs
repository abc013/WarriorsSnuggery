using System;
using WarriorsSnuggery.Physics;

namespace WarriorsSnuggery.Objects.Weapons
{
	class BulletWeapon : Weapon
	{
		readonly BulletProjectileType projectileType;
		readonly RayPhysics rayPhysics;

		Vector speed;
		Vector speedLeft;

		public BulletWeapon(World world, WeaponType type, Target target, Actor origin) : base(world, type, target, origin)
		{
			projectileType = (BulletProjectileType)type.Projectile;

			TargetPosition += getInaccuracy();

			var angle = (Position - TargetPosition).FlatAngle;
			if ((Position - TargetPosition).Dist > type.MaxRange * RangeModifier)
			{
				TargetPosition = Position + new CPos((int)(Math.Cos(angle) * type.MaxRange * RangeModifier), (int)(Math.Sin(angle) * type.MaxRange * RangeModifier), 0) + getInaccuracy();
				angle = (Position - TargetPosition).FlatAngle;
			}

			calculateStartSpeed(angle);

			if (!projectileType.FlyToTarget)
				TargetPosition = Position + new CPos((int)(Math.Cos(angle) * type.MaxRange * RangeModifier), (int)(Math.Sin(angle) * type.MaxRange * RangeModifier), 0) + getInaccuracy();

			if (projectileType.OrientateToTarget)
				Rotation = new VAngle(0, 0, angle);

			rayPhysics = new RayPhysics(world);
		}

		void calculateStartSpeed(float angle)
		{
			var x = (float)Math.Cos(angle) * projectileType.Speed;
			var y = (float)Math.Sin(angle) * projectileType.Speed;

			var zDiff = TargetHeight - Height;
			var dDiff = (int)(Position - TargetPosition).FlatDist;

			var angle2 = new CPos(-dDiff, -zDiff, 0).FlatAngle;
			var z = (float)Math.Sin(angle2) * projectileType.Speed;
			var plusZ = (dDiff / projectileType.Speed) * -projectileType.Force.Z / 2;

			speed = new Vector(x, y, z + plusZ);
		}

		public override void Tick()
		{
			base.Tick();

			if (World.Game.Editor)
				return;

			if (projectileType.OrientateToTarget)
				Rotation = new VAngle(0, 0, -(TargetPosition - GraphicPosition).FlatAngle);

			Move();

			if (projectileType.TrailParticles != null)
				World.Add(projectileType.TrailParticles.Create(World, Position, Height));
		}

		public void Move()
		{
			var beforePos = Position;
			var beforeHeight = Height;

			var curSpeed = speed + speedLeft;
			var x = (int)curSpeed.X;
			var y = (int)curSpeed.Y;
			var z = (int)curSpeed.Z;
			speedLeft = new Vector(curSpeed.X - x, curSpeed.Y - y, curSpeed.Z - z);

			Position = new CPos(Position.X + x, Position.Y + y, Position.Z);
			Height += z;
			speed += new Vector(projectileType.Force.X, projectileType.Force.Y, projectileType.Force.Z);

			if (Math.Abs(speed.X) > projectileType.MaxSpeed)
				speed = new Vector(Math.Sign(speed.X) * projectileType.MaxSpeed, speed.Y, speed.Z);
			if (Math.Abs(speed.Y) > projectileType.MaxSpeed)
				speed = new Vector(speed.X, Math.Sign(speed.Y) * projectileType.MaxSpeed, speed.Z);
			if (Math.Abs(speed.Z) > projectileType.MaxSpeed)
				speed = new Vector(speed.X, speed.Y, Math.Sign(speed.Z) * projectileType.MaxSpeed);

			if (Height < 0 || !World.IsInWorld(Position))
				Detonate(new Target(Position, 0));

			rayPhysics.Start = beforePos;
			rayPhysics.StartHeight = beforeHeight;
			rayPhysics.Target = Position;
			rayPhysics.TargetHeight = Height;
			rayPhysics.CalculateEnd(new[] { Origin });

			if ((beforePos - rayPhysics.End).Dist < (beforePos - Position).Dist)
				Detonate(new Target(rayPhysics.End, rayPhysics.EndHeight));
		}

		CPos getInaccuracy()
		{
			if (projectileType.Inaccuracy > 0)
			{
				var ranX = (Program.SharedRandom.Next(projectileType.Inaccuracy) - projectileType.Inaccuracy / 2) * InaccuracyModifier;
				var ranY = (Program.SharedRandom.Next(projectileType.Inaccuracy) - projectileType.Inaccuracy / 2) * InaccuracyModifier;

				return new CPos((int)ranX, (int)ranY, 0);
			}

			return CPos.Zero;
		}
	}
}
