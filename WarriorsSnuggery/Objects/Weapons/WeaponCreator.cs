using System.Collections.Generic;
using System.Linq;

namespace WarriorsSnuggery.Objects.Weapons
{
	public static class WeaponCreator
	{
		public static readonly Dictionary<string, WeaponType> Types = new Dictionary<string, WeaponType>();

		public static void Load(string directory, string file)
		{
			var weapons = RuleReader.FromFile(directory, file);

			foreach (var weapon in weapons)
				Types.Add(weapon.Key, new WeaponType(weapon.Children));
		}

		public static string GetName(WeaponType type)
		{
			return Types.First(t => t.Value == type).Key;
		}

		public static Weapon Create(World world, string name, CPos target, Actor origin, uint id = uint.MaxValue)
		{
			if (!Types.ContainsKey(name))
				throw new MissingInfoException(name);

			return Create(world, Types[name], new Target(target, 0), origin, id);
		}

		public static Weapon Create(World world, WeaponType type, Target target, Actor origin, uint id = uint.MaxValue)
		{
			if (id == uint.MaxValue)
				id = world.Game.NextWeaponID;

			if (type.Projectile is BeamProjectileType)
				return new BeamWeapon(world, type, target, origin, id);
			else if (type.Projectile is BulletProjectileType)
				return new BulletWeapon(world, type, target, origin, id);
			else if (type.Projectile is MagicProjectileType)
				return new MagicWeapon(world, type, target, origin, id);
			else
				return new InstantHitWeapon(world, type, target, origin, id);
		}

		public static Weapon Create(World world, WeaponInit init)
		{
			var type = init.Type;
			if (type.Projectile is BeamProjectileType)
				return new BeamWeapon(world, init);
			else if (type.Projectile is BulletProjectileType)
				return new BulletWeapon(world, init);
			else if (type.Projectile is MagicProjectileType)
				return new MagicWeapon(world, init);
			else
				return new InstantHitWeapon(world, init);
		}
	}
}
