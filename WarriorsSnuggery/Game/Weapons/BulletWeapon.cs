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
			var angle = Target.AngleToXY(Position);
			Target = Position + new CPos((int)(Math.Cos((angle * Math.PI) / 180) * Type.MaxRange), (int)(Math.Sin((angle * Math.PI) / 180) * Type.MaxRange), 0);

			Target += getInaccuracy();
		}
	}
}
