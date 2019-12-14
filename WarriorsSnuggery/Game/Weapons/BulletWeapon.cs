using System;

namespace WarriorsSnuggery.Objects
{
	class BulletWeapon : Weapon
	{
		float flatDistMoved;
		int speed;

		public BulletWeapon(World world, WeaponType type, Actor origin, CPos target) : this(world, type, origin.ActiveWeapon.WeaponOffsetPosition, target, origin) { }

		public BulletWeapon(World world, WeaponType type, CPos origin, CPos target, Actor originActor = null) : base(world, type, origin, target, originActor)
		{
			if (Type.Acceleration == 0)
				speed = Type.Speed;

			var angle = (Position - Target).FlatAngle;
			Target = Position + new CPos((int)(Math.Cos(angle) * Type.MaxRange), (int)(Math.Sin(angle) * Type.MaxRange), 0);

			Target += getInaccuracy();
		}

		public override void Tick()
		{
			base.Tick();

			if (Type.Acceleration != 0 && speed != Type.Speed)
			{
				speed += Type.Acceleration;
				if (speed > Type.Speed)
					speed = Type.Speed;
			}

			if (Type.OrientateToTarget)
				Rotation = new VAngle(0, 0, -(Target - Position).FlatAngle);

			Move(Target);
		}

		public void Move(CPos target)
		{
			var angle = (Position - target).FlatAngle;

			var x = Math.Cos(angle) * speed;
			var y = Math.Sin(angle) * speed;
			double z;
			if (Type.WeaponFireType == WeaponFireType.BULLET)
			{
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
				z = Math.Sin(angle2) * speed;
			}
			else
			{
				z = Type.Gravity;
			}

			var old = Position;
			Position = new CPos(Position.X + (int)x, Position.Y + (int)y, Position.Z);
			Physics.Position = Position;

			Height -= (int)z;
			if (Height < 0)
				Detonate();

			World.PhysicsLayer.UpdateSectors(this, updateSectors: false);

			if (World.CheckCollision(this, true, new[] { Origin }))
				Detonate();

			flatDistMoved += (Position - old).FlatDist;
			if (flatDistMoved > Type.MaxRange * RangeModifier || !World.IsInWorld(Position))
				Detonate();
		}
	}
}
