using System;
using System.Collections.Generic;
using WarriorsSnuggery.Physics;

namespace WarriorsSnuggery.Objects.Weapons
{
	class MagicWeapon : Weapon
	{
		readonly MagicProjectileType projectileType;
		readonly RayPhysics rayPhysics;

		Vector speed;
		Vector speedLeft;

		public MagicWeapon(World world, WeaponType type, Target target, Actor origin, uint id) : base(world, type, target, origin, id)
		{
			projectileType = (MagicProjectileType)type.Projectile;

			Angle = (Position - TargetPosition).FlatAngle;
			calculateSpeed();

			TargetPosition += getInaccuracy(projectileType.Inaccuracy);

			rayPhysics = new RayPhysics(world);
		}

		public MagicWeapon(World world, WeaponInit init) : base(world, init)
		{
			projectileType = (MagicProjectileType)Type.Projectile;

			speed = init.Convert("Speed", Vector.Zero);
			speedLeft = init.Convert("SpeedLeft", Vector.Zero);
			if (speed == Vector.Zero)
				calculateSpeed();

			rayPhysics = new RayPhysics(world);
		}

		public override void Tick()
		{
			base.Tick();

			if (World.Game.Editor)
				return;

			if (projectileType.OrientateToTarget)
				Rotation = new VAngle(0, 0, -(TargetPosition - GraphicPosition).FlatAngle);

			if (projectileType.FollowTarget)
			{
				TargetPosition = Target.Position;
				TargetHeight = Target.Height;
				calculateAngle();
				calculateSpeed();
			}

			Move();

			if (projectileType.TrailParticles != null)
				World.Add(projectileType.TrailParticles.Create(World, Position, Height));
		}

		void calculateAngle()
		{
			if (projectileType.Turbulence != 0)
				calculateTurbulence();

			var diff = (Position - TargetPosition).FlatAngle - Angle;

			if (Math.Abs(diff) > projectileType.FloatTurnSpeed)
				diff = Math.Sign(diff) * projectileType.FloatTurnSpeed;

			Angle += diff;
		}

		void calculateTurbulence()
		{
			var dist = (Position - TargetPosition).FlatDist;

			Angle += (float)(Program.SharedRandom.NextDouble() - 0.5f) * projectileType.Turbulence * dist / (Type.MaxRange * 1024f);
		}

		void calculateSpeed()
		{
			var x = (float)Math.Cos(Angle) * projectileType.Speed;
			var y = (float)Math.Sin(Angle) * projectileType.Speed;

			var zDiff = TargetHeight - Height;
			var dDiff = (int)(Position - TargetPosition).FlatDist;
			if (dDiff > Type.MaxRange)
				dDiff = Type.MaxRange;

			var angle2 = new CPos(-dDiff, -zDiff, 0).FlatAngle;
			var z = (float)Math.Sin(angle2) * projectileType.Speed;

			speed = new Vector(x, y, z);
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

		public override List<string> Save()
		{
			var list = base.Save();

			list.Add("Speed=" + speed);
			list.Add("SpeedLeft=" + speedLeft);

			return list;
		}
	}
}
