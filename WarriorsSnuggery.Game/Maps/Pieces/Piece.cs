using System;
using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Objects.Actors;
using WarriorsSnuggery.Objects.Particles;
using WarriorsSnuggery.Objects.Weapons;

namespace WarriorsSnuggery.Maps.Pieces
{
	public class Piece
	{
		public readonly int MapFormat;

		public readonly MPos Size;
		public readonly string Name;
		public readonly string InnerName;
		public readonly string Path;

		public uint MaxActorID => actors.Count > 0 ? actors.Max(n => n.ID) : 0;
		public uint MaxWeaponID => weapons.Count > 0 ? weapons.Max(n => n.ID) : 0;

		readonly ushort[] groundData;
		readonly short[] wallData;

		readonly List<ActorInit> actors = new List<ActorInit>();
		readonly List<WeaponInit> weapons = new List<WeaponInit>();
		readonly List<ParticleInit> particles = new List<ParticleInit>();

		public Piece(string innerName, string path, List<TextNode> nodes)
		{
			InnerName = innerName;
			Path = path;

			var fields = TypeLoader.GetFields(this);

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
								throw new InvalidPieceException($"unable to load actor '{actor.Key}' in piece '{Name}'.", e);
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
								throw new InvalidPieceException($"unable to load weapon '{weapon.Key}' in piece '{Name}'.", e);
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
								throw new InvalidPieceException($"unable to load particle '{particle.Key}' in piece '{Name}'.", e);
							}
						}
						break;
					case "MapFormat":
						TypeLoader.SetValue(this, fields, node);

						if (MapFormat != Constants.CurrentMapFormat)
							Log.LoaderWarning("Pieces", $"Attempting to load old map format {MapFormat} (current: {Constants.CurrentMapFormat})");

						break;
					default:
						TypeLoader.SetValue(this, fields, node);

						break;
				}
			}

			if (groundData.Length != Size.X * Size.Y)
				throw new InvalidPieceException($"The count of given terrains ({groundData.Length}) is not the size ({Size.X * Size.Y}) of the piece '{Name}'");

			if (wallData.Length != (Size.X + 1) * (Size.Y + 1) * 2 * 2)
				throw new InvalidPieceException($"The count of given walls ({groundData.Length}) is smaller as the size ({Size.X * Size.Y}) on the piece '{Name}'");
		}

		public void PlacePiece(MPos position, MapLoader loader)
		{
			// generate Terrain
			for (int y = 0; y < Size.Y; y++)
				for (int x = 0; x < Size.X; x++)
					loader.SetTerrain(x + position.X, y + position.Y, groundData[y * Size.X + x]);

			// generate Walls
			if (wallData.Length != 0)
			{
				var maxX = (Size.X + 1 + position.X) * 2;
				var maxY = (Size.Y + 1 + position.Y);
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

			// generate Actors
			foreach (var init in actors)
				loader.AddActor(init, position.ToCPos());

			// generate Weapons
			foreach (var init in weapons)
				loader.AddWeapon(init);

			// generate Particles
			foreach (var init in particles)
				loader.AddParticle(init);
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
