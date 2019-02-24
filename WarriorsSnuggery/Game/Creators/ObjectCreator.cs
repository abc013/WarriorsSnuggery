using System;
using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery
{
	public static class ActorCreator
	{
		public static void LoadTypes(string file)
		{
			var actors = RuleReader.Read(file);

			foreach(var actor in actors)
			{
				var name = actor.Key;
				TextureInfo idle = null;
				int idleFrames = 1;
				TextureInfo walk = null;
				int walkFrames = 0;
				TextureInfo attack = null;
				int attackFrames = 0;
				TextureInfo shadow = null;

				var offset = CPos.Zero;
				var scale = 1f;
				var height = 0;
				var startMana = 0;
				var physicalSize = 0;
				var physicalShape = Shape.NONE;
				
				WeaponType manaWeapon = null;
				var manaCost = 0;

				var partinfos = new List<PartInfo>();

				foreach(var child in actor.Children)
				{
					if (Loader.PartLoader.IsPart(child.Key))
					{
						var part = Loader.PartLoader.GetPart(child.Key, child.Children.ToArray());

						partinfos.Add(part);
						continue;
					}

					switch(child.Key)
					{
						case "Image":
							int frames;
							string type;
							var info = child.ToTextureInfoAnim(out frames, out type);

							switch(type)
							{
								default:
									idle = info;
									idleFrames = frames;
									break;
								case "walk":
									walk = info;
									walkFrames = frames;
									break;
								case "attack":
									attack = info;
									attackFrames = frames;
									break;
							}
							break;
						case "Offset":
							offset = child.ToCPos();

							break;
						case "Scale":
							scale = child.ToFloat();

							break;
						case "Height":
							height = child.ToInt();

							break;
						case "Mana":
							startMana = child.ToInt();

							foreach (var mana in child.Children)
							{
								switch (mana.Key)
								{
									case "Cost":
										manaCost = mana.ToInt();

										break;
									case "Weapon":
										manaWeapon = WeaponCreator.GetType(mana.Value);

										break;
								}
							}

							break;
						case "Physics":
							if (child.Children.Count > 0)
							{
								foreach(var physics in child.Children)
								{
									switch(physics.Key)
									{
										case "Shape":
											physicalShape = (Shape) physics.ToEnum(typeof(Shape));

											break;
										case "Size":
											physicalSize = physics.ToInt();

											break;
									}
								}
							}

							break;
					}
				}

				if (idle == null)
					throw new YamlMissingNodeException(actor.Key, "Image");

				AddType(new ActorType(idle, idleFrames, walk, walkFrames, attack, attackFrames, shadow, offset, scale, height, startMana, physicalSize, physicalShape, manaWeapon, manaCost, (Objects.Parts.PlayablePartInfo) partinfos.Find(p => p is Objects.Parts.PlayablePartInfo), partinfos.ToArray()), name);
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

		public static Actor Create(World world, string name, CPos position, int team = 0, bool isBot = false, bool isPlayer = false)
		{
			var type = GetType(name);
			return Create(world, type, position, team, isBot, isPlayer);
		}

		public static Actor Create(World world, ActorType type, CPos position, int team = 0, bool isBot = false, bool isPlayer = false)
		{
			return new Actor(world, type, position, Convert.ToUInt16(team), isBot, isPlayer);
		}
	}

	public static class WeaponCreator
	{
		public static void LoadTypes(string file)
		{
			var weapons = RuleReader.Read(file);

			foreach(var weapon in weapons)
			{
				var name = weapon.Key;
				TextureInfo info = null;
				TextureInfo smudge = null;
				var scale = 1f;
				var damage = 0;
				var speed = 0;
				var acceleration = 0;
				var reload = 0;
				WeaponFireType type = WeaponFireType.BULLET;
				ParticleType particleWhenExplode = null;
				var particleCountWhenExplode = 5;
				var inaccuracy = 0;
				var maxRange = 8192;
				var minRange = 512;
				var damageFalloff = FalloffType.CUBIC;
				var turnToTarget = true;
				var physicalShape = Shape.CIRCLE;
				var physicalSize = 100;

				foreach(var child in weapon.Children)
				{
					switch(child.Key)
					{
						case "Image":
							info = child.ToTextureInfo();

							break;
						case "Type":
							type = (WeaponFireType) child.ToEnum(typeof(WeaponFireType));

							break;
						case "Smudge":
							smudge = child.ToTextureInfo();

							break;
						case "Scale":
							scale = child.ToFloat();

							break;
						case "Damage":
							damage = child.ToInt();

							break;
						case "Reload":
							reload = child.ToInt();

							break;
						case "Speed":
							speed = child.ToInt();

							if (child.Children.Count > 0 && child.Children.Exists(c => c.Key == "Acceleration"))
								acceleration = child.Children.Find(c => c.Key == "Acceleration").ToInt();

							break;
						case "ExplodeParticles":
							particleWhenExplode = ParticleCreator.GetType(child.Value);

							if (child.Children.Count > 0 && child.Children.Exists(c => c.Key == "Count"))
								particleCountWhenExplode = child.Children.Find(c => c.Key == "Count").ToInt();

							break;
						case "Inaccuracy":
							inaccuracy = child.ToInt();

							break;
						case "MaximalRange":
							maxRange = child.ToInt();

							break;
						case "MinimalRange":
							minRange = child.ToInt();

							break;
						case "Falloff":
							damageFalloff = (FalloffType) child.ToEnum(typeof(FalloffType));

							break;
						case "RotateToTarget":
							turnToTarget = child.ToBoolean();

							break;
						case "Physics":
							if (child.Children.Count > 0)
							{
								foreach(var physics in child.Children)
								{
									switch(physics.Key)
									{
										case "Shape":
											physicalShape = (Shape) physics.ToEnum(typeof(Shape));

											break;
										case "Size":
											physicalSize = physics.ToInt();

											break;
									}
								}
							}

							break;
					}
				}

				if (info == null)
					throw new YamlMissingNodeException(weapon.Key, "Image");

				AddTypes(new WeaponType(info, smudge, scale, damage, speed, acceleration, reload, particleWhenExplode, particleCountWhenExplode, inaccuracy, maxRange, minRange, damageFalloff, type, turnToTarget, physicalShape, physicalSize), name);
			}
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

		public static Weapon Create(World world, string name, CPos position, CPos target)
		{
			var type = GetType(name);
			return Create(world, type, position, target);
		}

		public static Weapon Create(World world, WeaponType type, CPos position, CPos target)
		{
			Weapon weapon;
			switch (type.WeaponFireType)
			{
				case WeaponFireType.ROCKET:
					weapon = new RocketWeapon(world, type, position, target);
					break;
				case WeaponFireType.BEAM:
					weapon = new BeamWeapon(world, type, position, target);
					break;
				case WeaponFireType.BULLET:
				default:
					weapon = new BulletWeapon(world, type, position, target);
					break;
			}
			weapon.Scale *= type.Scale;
			return weapon;
		}

		public static Weapon Create(World world, WeaponType type, Actor origin, CPos target)
		{
			Weapon weapon;
			switch (type.WeaponFireType)
			{
				case WeaponFireType.ROCKET:
					weapon = new RocketWeapon(world, type, origin, target);
					break;
				case WeaponFireType.BEAM:
					weapon = new BeamWeapon(world, type, origin, target);
					break;
				case WeaponFireType.BULLET:
				default:
					weapon = new BulletWeapon(world, type, origin, target);
					break;
			}
			weapon.Scale *= type.Scale;
			return weapon;
		}
	}

	public static class ParticleCreator
	{
		public static void LoadTypes(string file)
		{
			var particles = RuleReader.Read(file);

			foreach(var particle in particles)
			{
				var name = particle.Key;
				TextureInfo info = null;
				var tick = 0;
				var dissolveTick = 0;
				var force = CPos.Zero;
				var rotation = 0;
				var ranVelocity = CPos.Zero;
				var scale = 1.0f;
				var ranScale = 0f;

				foreach(var child in particle.Children)
				{
					switch(child.Key)
					{
						case "Image":
							info = child.ToTextureInfo();

							break;
						case "Tick":
							tick = child.ToInt();

							break;
						case "DissolveTick":
							dissolveTick = child.ToInt();

							break;
						case "Force":
							force = child.ToCPos();

							if (child.Children.Count > 0)
								ranVelocity = child.Children.Find(c => c.Key == "Random").ToCPos();

							break;
						case "Rotation":
							rotation = child.ToInt();

							break;
						case "Scale":
							scale = child.ToFloat();

							if (child.Children.Count > 0)
								ranScale = child.Children.Find(c => c.Key == "Random").ToFloat();

							break;
					}
				}

				if (info == null)
					throw new YamlMissingNodeException(particle.Key, "Image");

				AddType(new ParticleType(info, tick, dissolveTick, force, rotation, ranVelocity, scale, ranScale), name);
			}
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

		public static Particle Create(string name, CPos position)
		{
			return Create(GetType(name), position);
		}

		public static Particle Create(ParticleType type, CPos position)
		{
			return new Particle(position, type);
		}
	}

	public static class TerrainCreator
	{
		public static void LoadTypes(string file)
		{
			var terrains = RuleReader.Read(file);

			foreach(var terrain in terrains)
			{
				var image = string.Empty;
				var speedModifier = 1f;
				var overlapHeight = -1;
				var spawnSmudge = true;
				var edge_Image = "";
				var edge_Image2 = "";
				var corner_Image = "";

				foreach(var child in terrain.Children)
				{
					switch(child.Key)
					{
						case "Image":
							image = child.Value;

							break;
						case "Speed":
							speedModifier = child.ToFloat();

							break;
						case "Overlaps":
							overlapHeight = child.ToInt();
							foreach(var rule in child.Children)
							{
								switch(rule.Key)
								{
									case "Edge":
										edge_Image = rule.Value;

										break;
									case "VerticalEdge":
										edge_Image2 = rule.Value;

										break;
									case "Corner":
										corner_Image = rule.Value;

										break;
								}
							}

							break;
						case "SpawnSmudge":
							spawnSmudge = child.ToBoolean();

							break;
					}
				}

				if (image == string.Empty)
					throw new YamlMissingNodeException(terrain.Key, "Image");

				var id = int.Parse(terrain.Key);

				AddType(new TerrainType(id, image, speedModifier, edge_Image != "", spawnSmudge, overlapHeight, edge_Image, corner_Image, edge_Image2));
			}
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

		public static Terrain Create(World world, WPos position, int ID)
		{
			return new Terrain(world, position, GetType(ID));
		}
	}

	public static class WallCreator
	{
		public static void LoadTypes(string file)
		{
			var walls = RuleReader.Read(file);

			foreach(var wall in walls)
			{
				string texture = string.Empty;
				bool blocks = true;
				bool destroyable = false;
				int height = 512;

				foreach(var child in wall.Children)
				{
					switch(child.Key)
					{
						case "Image":
							texture = child.Value;

							break;
						case "Blocks":
							blocks = child.ToBoolean();

							break;
						case "Destroyable":
							destroyable = child.ToBoolean();

							break;
						case "Height":
							height = child.ToInt();

							break;
					}
				}

				if (texture == string.Empty)
					throw new YamlMissingNodeException(wall.Key, "Image");

				var id = int.Parse(wall.Key);

				AddType(new WallType(id, texture, blocks, destroyable, height));
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

		public static Wall Create(WPos position, int ID)
		{
			return new Wall(position, GetType(ID));
		}
	}
}
