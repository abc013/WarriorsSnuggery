using System;
using System.Collections.Generic;

namespace WarriorsSnuggery.Objects.Weapons
{
	class MagicWeapon : Weapon
	{
		readonly MagicProjectileType projectileType;

		Vector speed;
		float angle;

		public MagicWeapon(World world, WeaponType type, Target target, Actor origin, uint id) : base(world, type, target, origin, id)
		{
			projectileType = (MagicProjectileType)type.Projectile;

			angle = (Position - TargetPosition).FlatAngle;
			calculateSpeed(angle);

			if ((Position - TargetPosition).Dist > type.MaxRange * RangeModifier)
				TargetPosition = clampToMaxRange(Position, angle);

			TargetPosition += getInaccuracy(projectileType.Inaccuracy);
		}

		public MagicWeapon(World world, WeaponInit init) : base(world, init)
		{
			projectileType = (MagicProjectileType)Type.Projectile;

			angle = (Position - TargetPosition).FlatAngle;

			speed = init.Convert("Speed", Vector.Zero);
			if (speed == Vector.Zero)
				calculateSpeed(angle);
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
				calculateSpeed(angle);
			}

			Move();

			if (projectileType.TrailParticles != null)
				World.Add(projectileType.TrailParticles.Create(World, Position, Height));
		}

		void calculateAngle()
		{
			if (projectileType.Turbulence != 0)
				calculateTurbulence();

			var diff = (Position - TargetPosition).FlatAngle - angle;

			if (Math.Abs(diff) > projectileType.FloatTurnSpeed)
				diff = Math.Sign(diff) * projectileType.FloatTurnSpeed;

			angle += diff;
		}

		void calculateTurbulence()
		{
			var dist = (Position - TargetPosition).FlatDist;

			angle += (float)(Program.SharedRandom.NextDouble() - 0.5f) * projectileType.Turbulence * dist / (Type.MaxRange * 1024f);
		}

		void calculateSpeed(float angle)
		{
			var x = (float)Math.Cos(angle) * projectileType.Speed;
			var y = (float)Math.Sin(angle) * projectileType.Speed;

			var zDiff = TargetHeight - Height;
			var dDiff = (int)(Position - TargetPosition).FlatDist;

			var angle2 = new CPos(-dDiff, -zDiff, 0).FlatAngle;
			var z = (float)Math.Sin(angle2) * projectileType.Speed;

			speed = new Vector(x, y, z);
		}

		public void Move()
		{
			var curSpeed = new CPos((int)speed.X, (int)speed.Y, (int)speed.Z);
			Position = new CPos(Position.X + curSpeed.X, Position.Y + curSpeed.Y, Position.Z);
			Physics.Position = Position;
			Height += curSpeed.Z;
			Physics.Height = Height;

			if (Height < 0 || !World.IsInWorld(Position))
				Detonate(new Target(Position, 0));

			World.PhysicsLayer.UpdateSectors(this, updateSectors: false);

			if (World.CheckCollision(this, Origin))
				Detonate(new Target(Position, Height));
		}

		public override List<string> Save()
		{
			var list = base.Save();

			list.Add("Speed=" + speed);
			
			return list;
		}
	}
}
