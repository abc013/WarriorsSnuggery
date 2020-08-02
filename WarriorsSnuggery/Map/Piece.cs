using System;
using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Objects;
using WarriorsSnuggery.Objects.Particles;
using WarriorsSnuggery.Objects.Weapons;

namespace WarriorsSnuggery.Maps
{
	public class Piece
	{
		public readonly int MapFormat;

		public readonly MPos Size;
		public readonly string Name;
		public readonly string InnerName;
		public readonly string Path;

		readonly ushort[] groundData;
		readonly short[] wallData;

		readonly List<ActorInit> actors;
		readonly List<WeaponInit> weapons;
		readonly List<ParticleInit> particles;

		Piece(int mapFormat, MPos size, ushort[] groundData, short[] wallData, string name, string innerName, string path, List<ActorInit> actors, List<WeaponInit> weapons, List<ParticleInit> particles)
		{
			MapFormat = mapFormat;
			Size = size;
			Name = name;
			InnerName = innerName;
			Path = path;

			this.groundData = groundData;
			this.wallData = wallData;
			this.actors = actors;
			this.weapons = weapons;
			this.particles = particles;
		}

		public static Piece Load(string innerName, string path, MiniTextNode[] nodes)
		{
			var mapFormat = 0;
			var size = MPos.Zero;

			var groundData = new ushort[0];
			var wallData = new short[0];

			var name = "unknown";
			var actors = new List<ActorInit>();
			var weapons = new List<WeaponInit>();
			var particles = new List<ParticleInit>();

			foreach (var rule in nodes)
			{
				switch (rule.Key)
				{
					case "MapFormat":
						mapFormat = rule.Convert<int>();

						break;
					case "Size":
						size = rule.Convert<MPos>();

						break;
					case "Terrain":
						groundData = rule.Convert<ushort[]>();

						break;
					case "Walls":
						wallData = rule.Convert<short[]>();

						break;
					case "Name":
						name = rule.Convert<string>();

						break;
					case "Actors":
						var actorNodes = rule.Children;

						foreach (var actor in actorNodes)
						{
							try
							{
								var id = uint.Parse(actor.Key);

								if (mapFormat == 0)
									actors.Add(new ActorInit(id, actor));
								else
									actors.Add(new ActorInit(id, actor.Children));
							}
							catch (Exception e)
							{
								throw new InvalidPieceException(string.Format(@"unable to load actor '{0}' in piece '{1}'.", actor.Key, name), e);
							}
						}
						break;
					case "Weapons":
						var weaponNodes = rule.Children;

						foreach (var weapon in weaponNodes)
						{
							try
							{
								var id = uint.Parse(weapon.Key);
								weapons.Add(new WeaponInit(id, weapon.Children));
							}
							catch (Exception e)
							{
								throw new InvalidPieceException(string.Format(@"unable to load weapon '{0}' in piece '{1}'.", weapon.Key, name), e);
							}
						}
						break;
					case "Particles":
						var particleNodes = rule.Children;

						foreach (var particle in particleNodes)
						{
							try
							{
								particles.Add(new ParticleInit(particle.Children));
							}
							catch (Exception e)
							{
								throw new InvalidPieceException(string.Format(@"unable to load particle '{0}' in piece '{1}'.", particle.Key, name), e);
							}
						}
						break;
				}
			}

			if (groundData.Length != size.X * size.Y)
				throw new InvalidPieceException(string.Format(@"The count of given terrains ({0}) is not the size ({1}) of the piece '{2}'", groundData.Length, size.X * size.Y, name));

			if (wallData.Length != (size.X + 1) * (size.Y + 1) * 2 * 2)
				throw new InvalidPieceException(string.Format(@"The count of given walls ({0}) is smaller as the size ({1}) on the piece '{2}'", groundData.Length, size.X * size.Y, name));

			return new Piece(mapFormat, size, groundData, wallData, name, innerName, path, actors, weapons, particles);
		}

		public void PlacePiece(MPos position, World world)
		{
			// generate Terrain
			for (int y = position.Y; y < (Size.Y + position.Y); y++)
			{
				for (int x = position.X; x < (Size.X + position.X); x++)
				{
					world.TerrainLayer.Set(TerrainCreator.Create(world, new MPos(x, y), groundData[(y - position.Y) * Size.X + (x - position.X)]));
				}
			}

			// generate Walls
			if (wallData.Length != 0)
			{
				var maxY = (Size.Y + 1 + position.Y);
				var maxX = (Size.X + 1 + position.X) * 2;
				for (int y = position.Y; y < maxY; y++)
				{
					for (int x = position.X * 2; x < maxX; x++)
					{
						var dataPos = (y - position.Y) * (Size.X + 1) * 2 + (x - position.X * 2);
						dataPos *= 2;

						if (wallData[dataPos] >= 0)
						{
							var wall = WallCreator.Create(new MPos(x, y), world.WallLayer, wallData[dataPos]);
							world.WallLayer.Set(wall);

							wall.Health = wallData[dataPos + 1];
						}
						else if (world.WallLayer.Walls[x, y] != null && x != position.X * 2 && y != position.Y && y != maxY - 1 && !(x >= maxX - 2))
							world.WallLayer.Remove(new MPos(x, y));
					}
				}
			}

			if (!world.Map.Type.FromSave)
			{
				if (actors.Any())
					world.Game.CurrentActorID = actors.Max(n => n.ID) + 1;
				if (weapons.Any())
					world.Game.CurrentWeaponID = weapons.Max(n => n.ID) + 1;
			}

			var generatedActors = new List<Actor>();
			// generate Actors
			foreach (var init in actors)
			{
				var actor = ActorCreator.Create(world, init, !world.Map.Type.FromSave, position.ToCPos());
				generatedActors.Add(actor);
				world.Add(actor);
			}

			if (world.Map.Type.FromSave)
				world.LocalPlayer = world.ActorLayer.ToAdd().Find(a => a.IsPlayer);

			// generate Weapons
			foreach (var weapon in weapons)
				world.Add(WeaponCreator.Create(world, weapon));

			// generate Particles
			foreach (var particle in particles)
				world.Add(ParticleCreator.Create(world, particle));

			foreach (var actor in generatedActors)
				actor.OnLoad();
		}

		public bool IsInMap(MPos position, MPos mapSize)
		{
			if (Size.X + position.X > mapSize.X)
				return false;
			if (Size.Y + position.Y > mapSize.Y)
				return false;

			return true;
		}
	}
}
