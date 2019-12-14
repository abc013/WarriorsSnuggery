using System;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.Objects.Weapons
{
	class BulletWeapon : Weapon
	{
		readonly BulletProjectileType projectileType;

		float flatDistMoved;
		int speed;

		public BulletWeapon(World world, WeaponType type, CPos origin, CPos target, Actor originActor) : base(world, type, origin, target, originActor)
		{
			projectileType = (BulletProjectileType)type.Projectile;
			speed = projectileType.Speed;

			var angle = (Position - Target).FlatAngle;

			if (projectileType.OrientateToTarget)
				Rotation = new VAngle(0, 0, angle);

			Target = Position + new CPos((int)(Math.Cos(angle) * type.MaxRange), (int)(Math.Sin(angle) * type.MaxRange), 0);

			Target += getInaccuracy();
		}

		public override void Tick()
		{
			base.Tick();

			if (projectileType.OrientateToTarget)
				Rotation = new VAngle(0, 0, -(Target - Position).FlatAngle);

			Move(Target);
		}

		public void Move(CPos target)
		{
			var angle = (Position - target).FlatAngle;

			var x = Math.Cos(angle) * speed;
			var y = Math.Sin(angle) * speed;
			double z;

			int zDiff;
			int dDiff;
			float angle2;
			if (TargetActor != null)
			{
				zDiff = Height - TargetActor.Height;
				dDiff = (int)(Position - TargetActor.Position).FlatDist;
			}
			else
			{
				zDiff = Height;
				dDiff = (int)(Position - Target).FlatDist;
			}
			angle2 = new CPos(-dDiff, -zDiff, 0).FlatAngle;
			z = Math.Sin(angle2) * speed; // TODO add gravity

			var old = Position;
			Position = new CPos(Position.X + (int)x, Position.Y + (int)y, Position.Z);
			Physics.Position = Position;

			Height -= (int)z;
			if (Height < 0)
				Detonate();

			World.PhysicsLayer.UpdateSectors(this, updateSectors: false);

			if (World.CheckCollision(this, false, new[] { Origin }))
				Detonate();

			flatDistMoved += (Position - old).FlatDist;
			if (flatDistMoved > Type.MaxRange * RangeModifier || !World.IsInWorld(Position))
				Detonate();
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
