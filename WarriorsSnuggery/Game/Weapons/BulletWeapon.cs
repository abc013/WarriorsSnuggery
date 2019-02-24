/*
 * User: Andreas
 * Date: 25.11.2017
 * 
 */
using System;

namespace WarriorsSnuggery.Objects
{
	class BulletWeapon : Weapon
	{
		public BulletWeapon(World world, WeaponType type, CPos origin, CPos target) : base(world, type, origin, target)
		{
			setNewTarget();
		}

		public BulletWeapon(World world, WeaponType type, Actor origin, CPos target) : base(world, type, origin, target)
		{
			setNewTarget();
		}

		void setNewTarget()
		{
			var angle = Target.GetAngleToXY(Position);
			Target = Position + new CPos((int)(Math.Cos((angle * Math.PI) / 180) * Type.MaxRange), (int)(Math.Sin((angle * Math.PI) / 180) * Type.MaxRange), 0);

			if (Type.Inaccuracy > 0)
			{
				var ranX = Program.SharedRandom.Next(Type.Inaccuracy) - Type.Inaccuracy / 2;
				var ranY = Program.SharedRandom.Next(Type.Inaccuracy) - Type.Inaccuracy / 2;

				Target += new CPos(ranX, ranY, 0);
			}
		}
	}
}
