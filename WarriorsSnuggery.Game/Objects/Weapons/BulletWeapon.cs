﻿using System;
using System.Collections.Generic;
using WarriorsSnuggery.Objects.Weapons.Projectiles;
using WarriorsSnuggery.Physics;

namespace WarriorsSnuggery.Objects.Weapons
{
	class BulletWeapon : Weapon
	{
		readonly BulletProjectile projectile;
		readonly RayPhysics rayPhysics;

		[Save("Speed")]
		Vector speed;
		[Save("SpeedLeft")]
		Vector speedLeft;

		public BulletWeapon(World world, WeaponType type, Target target, Actor origin, uint id) : base(world, type, target, origin, id)
		{
			projectile = (BulletProjectile)type.Projectile;

			var angle = (Position - TargetPosition).FlatAngle;

			Angle = angle;

			TargetPosition += getInaccuracy(projectile.Inaccuracy);

			calculateStartSpeed();

			if (projectile.OrientateToTarget)
				Rotation = new VAngle(0, 0, Angle);

			rayPhysics = new RayPhysics(world);
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

			rayPhysics = new RayPhysics(world);
		}

		void calculateStartSpeed()
		{
			var x = (float)Math.Cos(Angle) * projectile.Speed;
			var y = (float)Math.Sin(Angle) * projectile.Speed;

			var zDiff = TargetHeight - Height;
			var dDiff = (int)(Position - TargetPosition).FlatDist;
			if (dDiff > Type.MaxRange)
				dDiff = Type.MaxRange;

			var angle2 = new CPos(-dDiff, -zDiff, 0).FlatAngle;
			var z = (float)Math.Sin(angle2) * projectile.Speed;
			var plusZ = (int)((dDiff / (float)projectile.Speed) * -projectile.Force.Z / 2f);

			speed = new Vector(x, y, z + plusZ);
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

			Position = new CPos(Position.X + x, Position.Y + y, Position.Z);
			Height += z;
			speed += new Vector(projectile.Force.X, projectile.Force.Y, projectile.Force.Z);

			if (Math.Abs(speed.X) > projectile.MaxSpeed)
				speed = new Vector(Math.Sign(speed.X) * projectile.MaxSpeed, speed.Y, speed.Z);
			if (Math.Abs(speed.Y) > projectile.MaxSpeed)
				speed = new Vector(speed.X, Math.Sign(speed.Y) * projectile.MaxSpeed, speed.Z);
			if (Math.Abs(speed.Z) > projectile.MaxSpeed)
				speed = new Vector(speed.X, speed.Y, Math.Sign(speed.Z) * projectile.MaxSpeed);

			if (Height < 0 || !World.IsInWorld(Position))
				Detonate(new Target(Position, 0));

			rayPhysics.Start = beforePos;
			rayPhysics.StartHeight = beforeHeight;
			rayPhysics.Target = Position;
			rayPhysics.TargetHeight = Height;
			rayPhysics.CalculateEnd(new[] { Origin.Physics });

			if ((beforePos - rayPhysics.End).Dist < (beforePos - Position).Dist)
				Detonate(new Target(rayPhysics.End, rayPhysics.EndHeight));
		}

		public override List<string> Save()
		{
			var list = base.Save();
			list.AddRange(WorldSaver.GetSaveFields(this, false));

			return list;
		}
	}
}
