using System;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Objects.Actors;
using WarriorsSnuggery.Objects.Weapons.Projectiles;
using WarriorsSnuggery.Physics;

namespace WarriorsSnuggery.Objects.Weapons
{
	class MagicWeapon : Weapon
	{
		readonly MagicProjectile projectile;
		readonly PhysicsRay ray;

		[Save("Speed")]
		Vector speed;
		[Save("SpeedLeft")]
		Vector speedLeft;

		public MagicWeapon(World world, WeaponType type, Target target, Actor origin, uint id) : base(world, type, target, origin, id)
		{
			projectile = (MagicProjectile)type.Projectile;

			TargetPosition += getInaccuracy(projectile.Inaccuracy);
			Angle = (Position - TargetPosition).FlatAngle;

			calculateSpeed();

			ray = new PhysicsRay(world);
		}

		public MagicWeapon(World world, WeaponInit init) : base(world, init)
		{
			projectile = (MagicProjectile)Type.Projectile;

			TargetPosition += getInaccuracy(projectile.Inaccuracy);
			Angle = (Position - TargetPosition).FlatAngle;

			calculateSpeed();

			ray = new PhysicsRay(world);

			Load(init);
		}

		public override void Load(TextNodeInitializer initializer)
		{
			base.Load(initializer);
			initializer.SetSaveFields(this, false);
		}

		public override void Tick()
		{
			base.Tick();

			if (World.Game.Editor)
				return;

			if (projectile.OrientateToTarget)
				Rotation = new VAngle(0, 0, -(TargetPosition - GraphicPosition).FlatAngle);

			if (projectile.FollowTarget)
			{
				TargetPosition = Target.Position;
				calculateAngle();
				calculateSpeed();
			}

			Move();

			if (projectile.TrailParticles != null)
				World.Add(projectile.TrailParticles.Create(World, Position));
		}

		void calculateAngle()
		{
			var diff = WarriorsSnuggery.Angle.Diff((Position - TargetPosition).FlatAngle, Angle);
			if (Math.Abs(diff) > projectile.ArcTurnSpeed)
				diff = Math.Sign(diff) * projectile.ArcTurnSpeed;

			Angle += diff;

			if (projectile.Turbulence != 0)
				calculateTurbulence();
		}

		void calculateTurbulence()
		{
			var dist = (Position - TargetPosition).FlatDist;

			Angle += (float)(Program.SharedRandom.NextDouble() - 0.5f) * projectile.Turbulence * dist / (Type.MaxRange * 1024f);
		}

		void calculateSpeed()
		{
			var flatSpeed = Vector.FromFlatAngle(Angle, projectile.Speed);

			var zDiff = TargetPosition.Z - Position.Z;
			var dDiff = (int)(Position - TargetPosition).FlatDist;
			if (dDiff > Type.MaxRange * RangeModifier)
				dDiff = (int)(Type.MaxRange * RangeModifier);

			var angle2 = new CPos(-dDiff, -zDiff, 0).FlatAngle;
			var z = MathF.Sin(angle2) * projectile.Speed;

			speed = new Vector(flatSpeed.X, flatSpeed.Y, z);
		}

		public void Move()
		{
			var beforePos = Position;

			var curSpeed = speed + speedLeft;
			var x = (int)curSpeed.X;
			var y = (int)curSpeed.Y;
			var z = (int)curSpeed.Z;
			DistanceTravelled += (int)new CPos(x, y, 0).FlatDist;

			speedLeft = new Vector(curSpeed.X - x, curSpeed.Y - y, curSpeed.Z - z);

			Position += new CPos(x, y, z);

			if (OnGround && z < 0 || !World.IsInWorld(Position))
				Detonate(new Target(Position));

			ray.Start = beforePos;
			ray.Target = Position;
			ray.CalculateEnd(Origin.Physics, onlyToTarget: true);

			if ((beforePos - ray.End).SquaredFlatDist < (beforePos - Position).SquaredFlatDist)
				Detonate(new Target(ray.End));
		}

		public override TextNodeSaver Save()
		{
			var saver = base.Save();
			saver.AddSaveFields(this, false);

			return saver;
		}
	}
}
