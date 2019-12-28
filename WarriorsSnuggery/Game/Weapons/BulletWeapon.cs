using System;

namespace WarriorsSnuggery.Objects.Weapons
{
	class BulletWeapon : Weapon
	{
		readonly BulletProjectileType projectileType;

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

			if (projectileType.OrientateToTarget)
				Rotation = new VAngle(0, 0, -(TargetPosition - GraphicPosition).FlatAngle);

			Move();

			if (projectileType.TrailParticles != null)
				World.Add(projectileType.TrailParticles.Create(World, Position, Height));
		}

		public void Move()
		{
			var curSpeed = speed + speedLeft;
			var x = (int)curSpeed.X;
			var y = (int)curSpeed.Y;
			var z = (int)curSpeed.Z;
			speedLeft = new Vector(curSpeed.X - x, curSpeed.Y - y, curSpeed.Z - z);

			Position = new CPos(Position.X + x, Position.Y + y, Position.Z);
			Physics.Position = Position;
			Height += z;
			Physics.Height = Height;
			speed += new Vector(projectileType.Force.X, projectileType.Force.Y, projectileType.Force.Z);

			if (speed.X > projectileType.MaxSpeed)
				speed = new Vector(projectileType.MaxSpeed, speed.Y, speed.Z);
			if (speed.Y > projectileType.MaxSpeed)
				speed = new Vector(speed.X, projectileType.MaxSpeed, speed.Z);
			if (speed.Z > projectileType.MaxSpeed)
				speed = new Vector(speed.X, speed.Y, projectileType.MaxSpeed);

			if (Height < 0 || !World.IsInWorld(Position))
				Detonate(new Target(Position, 0));

			World.PhysicsLayer.UpdateSectors(this, updateSectors: false);

			if (World.CheckCollision(this, false, new[] { Origin }))
				Detonate(new Target(Position, Height));
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
