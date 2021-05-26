using System;
using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Objects.Actors;

namespace WarriorsSnuggery.Objects.Weapons
{
	public static class WeaponCreator
	{
		public static readonly Dictionary<string, WeaponType> Types = new Dictionary<string, WeaponType>();

		public static void Load(List<TextNode> nodes)
		{
			foreach (var node in nodes)
				Types.Add(node.Key, new WeaponType(node.Children));
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

			return (Weapon)Activator.CreateInstance(getWeaponType(type), new object[] { world, type, target, origin, id });
		}

		public static Weapon Create(World world, WeaponInit init)
		{
			return (Weapon)Activator.CreateInstance(getWeaponType(init.Type), new object[] { world, init });
		}

		static Type getWeaponType(WeaponType type)
		{
			var name = type.Projectile.GetType().Name[..^10];

			return Type.GetType("WarriorsSnuggery.Objects.Weapons." + name + "Weapon", false, true);
		}
	}
}
