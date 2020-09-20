using System;
using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Loader;
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

		readonly List<ActorInit> actors = new List<ActorInit>();
		readonly List<WeaponInit> weapons = new List<WeaponInit>();
		readonly List<ParticleInit> particles = new List<ParticleInit>();

		public Piece(string innerName, string path, List<MiniTextNode> nodes)
		{
			InnerName = innerName;
			Path = path;

			var fields = PartLoader.GetFields(this);

			foreach (var node in nodes)
			{
				switch (node.Key)
				{
					case "Terrain":
						groundData = node.Convert<ushort[]>();

						break;
					case "Walls":
						wallData = node.Convert<short[]>();

						break;
					case "Actors":
						foreach (var actor in node.Children)
						{
							try
							{
								var id = uint.Parse(actor.Key);

								if (MapFormat == 0)
									actors.Add(new ActorInit(id, actor));
								else
									actors.Add(new ActorInit(id, actor.Children));
							}
							catch (Exception e)
							{
								throw new InvalidPieceException(string.Format(@"unable to load actor '{0}' in piece '{1}'.", actor.Key, Name), e);
							}
						}
						break;
					case "Weapons":
						foreach (var weapon in node.Children)
						{
							try
							{
								var id = uint.Parse(weapon.Key);
								weapons.Add(new WeaponInit(id, weapon.Children));
							}
							catch (Exception e)
							{
								throw new InvalidPieceException(string.Format(@"unable to load weapon '{0}' in piece '{1}'.", weapon.Key, Name), e);
							}
						}
						break;
					case "Particles":
						foreach (var particle in node.Children)
						{
							try
							{
								particles.Add(new ParticleInit(particle.Children));
							}
							catch (Exception e)
							{
								throw new InvalidPieceException(string.Format(@"unable to load particle '{0}' in piece '{1}'.", particle.Key, Name), e);
							}
						}
						break;
					default:
						PartLoader.SetValue(this, fields, node);

						break;
				}
			}

			if (groundData.Length != Size.X * Size.Y)
				throw new InvalidPieceException(string.Format(@"The count of given terrains ({0}) is not the size ({1}) of the piece '{2}'", groundData.Length, Size.X * Size.Y, Name));

			if (wallData.Length != (Size.X + 1) * (Size.Y + 1) * 2 * 2)
				throw new InvalidPieceException(string.Format(@"The count of given walls ({0}) is smaller as the size ({1}) on the piece '{2}'", groundData.Length, Size.X * Size.Y, Name));
		}

		public void PlacePiece(MPos position, MapLoader loader, World world)
		{
			// generate Terrain
			for (int y = position.Y; y < (Size.Y + position.Y); y++)
				for (int x = position.X; x < (Size.X + position.X); x++)
					loader.SetTerrain(x, y, groundData[(y - position.Y) * Size.X + (x - position.X)]);

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
							loader.SetWall(x, y, wallData[dataPos], wallData[dataPos + 1]);
						else if (loader.WallExists(x, y) && x != position.X * 2 && y != position.Y && y != maxY - 1 && !(x >= maxX - 2))
							loader.SetWall(x, y, 0, 0);
					}
				}
			}

			if (!loader.FromSave)
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
				var actor = loader.AddActor(init, !loader.FromSave, position.ToCPos());
				generatedActors.Add(actor);
			}

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
