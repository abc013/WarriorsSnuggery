using System;
using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Objects;
using WarriorsSnuggery.Objects.Particles;
using WarriorsSnuggery.Objects.Parts;
using WarriorsSnuggery.Objects.Weapons;

namespace WarriorsSnuggery
{
	public static class ActorCreator
	{
		public static void Load(string directory, string file)
		{
			var actors = RuleReader.Read(directory, file);

			foreach (var actor in actors)
			{
				var name = actor.Key;

				var partinfos = new List<PartInfo>();

				foreach (var child in actor.Children)
				{
					if (Loader.PartLoader.IsPart(child.Key))
					{
						var part = Loader.PartLoader.GetPart(child.Key, child.Children.ToArray());

						partinfos.Add(part);
						continue;
					}
					else
					{
						throw new YamlUnknownNodeException(child.Key, name);
					}
				}

				var physics = (PhysicsPartInfo)partinfos.Find(p => p is PhysicsPartInfo);
				var playable = (PlayablePartInfo)partinfos.Find(p => p is PlayablePartInfo);

				AddType(new ActorType(physics, playable, partinfos.ToArray()), name);
			}
		}

		static readonly Dictionary<string, ActorType> types = new Dictionary<string, ActorType>();

		public static void AddType(ActorType info, string name)
		{
			types.Add(name, info);
		}

		public static string GetName(ActorType type)
		{
			return types.FirstOrDefault(t => t.Value == type).Key;
		}

		public static string[] GetNames()
		{
			return types.Keys.ToArray();
		}

		public static ActorType GetType(string name)
		{
			if (!types.ContainsKey(name))
				throw new MissingInfoException(name);

			return types[name];
		}

		public static Actor Create(World world, string name, CPos position, byte team = 0, bool isBot = false, bool isPlayer = false, float health = 1f)
		{
			var type = GetType(name);
			return Create(world, type, position, team, isBot, isPlayer, health);
		}

		public static Actor Create(World world, ActorType type, CPos position, byte team = 0, bool isBot = false, bool isPlayer = false, float health = 1f)
		{
			var actor = new Actor(world, type, position, team, isBot, isPlayer);
			if (actor.Health != null)
				actor.Health.HP = (int)(actor.Health.HP * health);

			return actor;
		}
	}

	public static class WeaponCreator
	{
		public static void Load(string directory, string file)
		{
			var weapons = RuleReader.Read(directory, file);

			foreach (var weapon in weapons)
				AddTypes(new WeaponType(weapon.Children.ToArray()), weapon.Key);
		}

		static readonly Dictionary<string, WeaponType> types = new Dictionary<string, WeaponType>();

		public static string[] GetNames()
		{
			return types.Keys.ToArray();
		}

		public static string GetName(WeaponType type)
		{
			return types.FirstOrDefault(t => t.Value == type).Key;
		}

		public static void AddTypes(WeaponType info, string name)
		{
			types.Add(name, info);
		}

		public static WeaponType GetType(string name)
		{
			if (!types.ContainsKey(name))
				throw new MissingInfoException(name);

			return types[name];
		}

		public static Weapon Create(World world, string name, CPos target, Actor origin)
		{
			var type = GetType(name);
			return create(world, type, new Target(target, 0), origin);
		}

		public static Weapon Create(World world, WeaponType type, Target target, Actor origin)
		{
			return create(world, type, target, origin);
		}

		static Weapon create(World world, WeaponType type, Target target, Actor origin)
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

	public static class ParticleCreator
	{
		public static void Load(string directory, string file)
		{
			var nodes = RuleReader.Read(directory, file);

			foreach (var node in nodes)
				AddType(new ParticleType(node.Children.ToArray()), node.Key);
		}

		static readonly Dictionary<string, ParticleType> types = new Dictionary<string, ParticleType>();

		public static string[] GetNames()
		{
			return types.Keys.ToArray();
		}

		public static string GetName(ParticleType type)
		{
			return types.FirstOrDefault(t => t.Value == type).Key;
		}

		public static void AddType(ParticleType info, string name)
		{
			types.Add(name, info);
		}

		public static ParticleType GetType(string name)
		{
			if (!types.ContainsKey(name))
				throw new MissingInfoException(name);

			return types[name];
		}

		public static Particle Create(string name, CPos position, int height, Random random)
		{
			return Create(GetType(name), position, height, random);
		}

		public static Particle Create(ParticleType type, CPos position, int height, Random random)
		{
			return new Particle(position, height, type, random);
		}
	}

	public static class TerrainCreator
	{
		public static void LoadTypes(string directory, string file)
		{
			var terrains = RuleReader.Read(directory, file);

			foreach (var terrain in terrains)
				AddType(new TerrainType(ushort.Parse(terrain.Key), terrain.Children.ToArray()));
		}

		static readonly Dictionary<int, TerrainType> types = new Dictionary<int, TerrainType>();

		public static int[] GetIDs()
		{
			return types.Keys.ToArray();
		}

		public static int GetID(TerrainType type)
		{
			return types.FirstOrDefault(t => t.Value == type).Key;
		}

		public static void AddType(TerrainType type)
		{
			types.Add(type.ID, type);
		}

		public static TerrainType GetType(int ID)
		{
			if (!types.ContainsKey(ID))
				throw new MissingInfoException(ID.ToString());

			return types[ID];
		}

		public static Terrain Create(World world, MPos position, int ID)
		{
			return new Terrain(world, position, GetType(ID));
		}
	}

	public static class WallCreator
	{
		public static void Load(string directory, string file)
		{
			var walls = RuleReader.Read(directory, file);

			foreach (var wall in walls)
			{
				var id = int.Parse(wall.Key);

				AddType(new WallType(id, wall.Children.ToArray()));
			}
		}

		static readonly Dictionary<int, WallType> types = new Dictionary<int, WallType>();

		public static int[] GetIDs()
		{
			return types.Keys.ToArray();
		}

		public static int GetID(WallType type)
		{
			return types.FirstOrDefault(t => t.Value == type).Key;
		}

		public static void AddType(WallType type)
		{
			types.Add(type.ID, type);
		}

		public static WallType GetType(int ID)
		{
			if (!types.ContainsKey(ID))
				throw new MissingInfoException(ID.ToString());

			return types[ID];
		}

		public static Wall Create(MPos position, WallLayer layer, int ID)
		{
			return new Wall(position, layer, GetType(ID));
		}
	}
}
