using System.Collections.Generic;

namespace WarriorsSnuggery.Objects.Weapons
{
	public static class WeaponCreator
	{
		public static readonly Dictionary<string, WeaponType> Types = new Dictionary<string, WeaponType>();

		public static void Load(string directory, string file)
		{
			var weapons = RuleReader.Read(directory, file);

			foreach (var weapon in weapons)
				Types.Add(weapon.Key, new WeaponType(weapon.Children.ToArray()));
		}

		public static Weapon Create(World world, string name, CPos target, Actor origin)
		{
			if (!Types.ContainsKey(name))
				throw new MissingInfoException(name);

			return Create(world, Types[name], new Target(target, 0), origin);
		}

		public static Weapon Create(World world, WeaponType type, Target target, Actor origin)
		{
			if (type.Projectile is BeamProjectileType)
				return new BeamWeapon(world, type, target, origin);
			else if (type.Projectile is BulletProjectileType)
				return new BulletWeapon(world, type, target, origin);
			else if (type.Projectile is MagicProjectileType)
				return new MagicWeapon(world, type, target, origin);
			else
				return new InstantHitWeapon(world, type, target, origin);
		}
	}
}
