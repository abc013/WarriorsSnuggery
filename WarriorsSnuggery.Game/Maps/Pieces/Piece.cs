using System;
using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Objects;
using WarriorsSnuggery.Objects.Actors;
using WarriorsSnuggery.Objects.Particles;
using WarriorsSnuggery.Objects.Weapons;

namespace WarriorsSnuggery.Maps.Pieces
{
	public class Piece
	{
		public readonly int MapFormat;

		public readonly string Filepath;
		public readonly PackageFile PackageFile;

		public readonly string Name;
		public readonly MPos Size;

		public uint MaxActorID => actors.Count > 0 ? actors.Max(n => n.ID) : 0;
		public uint MaxWeaponID => weapons.Count > 0 ? weapons.Max(n => n.ID) : 0;

		readonly ushort[] groundData;

		readonly List<WallInit> walls = new List<WallInit>();
		readonly List<ActorInit> actors = new List<ActorInit>();
		readonly List<WeaponInit> weapons = new List<WeaponInit>();
		readonly List<ParticleInit> particles = new List<ParticleInit>();

		public Piece(PackageFile packageFile, string filepath)
		{
			PackageFile = packageFile;
			Filepath = filepath;

			var nodes = TextNodeLoader.FromFilepath(filepath);
			var fields = TypeLoader.GetFields(this);

			foreach (var node in nodes)
			{
				switch (node.Key)
				{
					case "Terrain":
						groundData = node.Convert<ushort[]>();

						break;
					case "Walls":
						if (MapFormat < 4)
						{
							var wallData = node.Convert<short[]>();

							for (uint i = 0; i < wallData.Length; i += 2)
							{
								if (wallData[i] < 0)
									continue;

								var wallID = i / 2;
								var x = ((int)wallID % ((Size.X + 1) * 2));
								var y = ((int)wallID / ((Size.X + 1) * 2));

								walls.Add(new WallInit(wallID, new WPos(x / 2, y, x % 2 == 1), wallData[i], wallData[i + 1]));
							}
						}
						else
						{
							foreach (var wall in node.Children)
							{
								var id = uint.Parse(wall.Key);

								walls.Add(new WallInit(id, wall.Children, MapFormat));
							}
						}

						break;
					case "Actors":
						foreach (var actor in node.Children)
						{
							try
							{
								var id = uint.Parse(actor.Key);

								var init = MapFormat == 0 ? new ActorInit(id, actor) : new ActorInit(id, actor.Children, MapFormat);

								if (init.Type != null)
									actors.Add(init);
								else if (Settings.LoadSoft)
									Log.LoaderWarning("Pieces", $"[{PackageFile}] Attempted to load actor {id} of nonexistent type. Skipping.");
								else
									throw new InvalidNodeException($"[{PackageFile}] Attempted to load actor {id} of nonexistant type.");
							}
							catch (Exception e)
							{
								throw new InvalidPieceException($"[{PackageFile}] Unable to load actor '{actor.Key}'.", e);
							}
						}
						break;
					case "Weapons":
						foreach (var weapon in node.Children)
						{
							try
							{
								var id = uint.Parse(weapon.Key);
								var init = new WeaponInit(id, weapon.Children, MapFormat);

								if (init.Type != null)
									weapons.Add(init);
								else if (Settings.LoadSoft)
									Log.LoaderWarning("Pieces", $"[{PackageFile}] Attempted to load weapon {id} of nonexistent type. Skipping.");
								else
									throw new InvalidNodeException($"[{PackageFile}] Attempted to load weapon {id} of nonexistant type.");

							}
							catch (Exception e)
							{
								throw new InvalidPieceException($"[{PackageFile}] Unable to load weapon '{weapon.Key}'.", e);
							}
						}
						break;
					case "Particles":
						foreach (var particle in node.Children)
						{
							try
							{
								var init = new ParticleInit(particle.Children, MapFormat);

								if (init.Type != null)
									particles.Add(init);
								else if (Settings.LoadSoft)
									Log.LoaderWarning("Pieces", $"[{PackageFile}] Attempted to load particle of nonexistent type. Skipping.");
								else
									throw new InvalidNodeException($"[{PackageFile}] Attempted to load particle of nonexistant type.");
							}
							catch (Exception e)
							{
								throw new InvalidPieceException($"[{PackageFile}] Unable to load particle '{particle.Key}'.", e);
							}
						}
						break;
					case "MapFormat":
						TypeLoader.SetValue(this, fields, node);

						if (MapFormat != Constants.CurrentMapFormat)
							Log.LoaderWarning("Pieces", $"[{PackageFile}] Attempting to load old map format {MapFormat} (current: {Constants.CurrentMapFormat})");

						break;
					default:
						TypeLoader.SetValue(this, fields, node);

						break;
				}
			}

			if (groundData.Length != Size.X * Size.Y)
				throw new InvalidPieceException($"The count of given terrains ({groundData.Length}) is not the size ({Size.X * Size.Y}) of the piece '{Name}'");
		}

		public void PlacePiece(MPos position, MapLoader loader)
		{
			// generate Terrain
			for (int y = 0; y < Size.Y; y++)
			{
				for (int x = 0; x < Size.X; x++)
				{
					var id = groundData[y * Size.X + x];
					if (Settings.LoadSoft && !TerrainCache.Types.ContainsKey(id))
					{
						Log.LoaderWarning("Pieces", $"[{PackageFile}] Attempted to load terrain of nonexistent type {id}. Skipping.");
						continue;
					}

					loader.SetTerrain(x + position.X, y + position.Y, id);
				}
			}

			// generate Walls
			var offset = new WPos(position.X, position.Y, false);
			var maxX = (Size.X + 1) * 2 + offset.X;
			var maxY = (Size.Y + 1) + offset.Y;

			foreach (var wall in walls)
			{
				var x = offset.X + wall.Position.X;
				var y = offset.Y + wall.Position.Y;
				var id = wall.TypeID;

				if (id >= 0)
				{
					if (Settings.LoadSoft && !WallCache.Types.ContainsKey(id))
					{
						Log.LoaderWarning("Pieces", $"[{PackageFile}] Attempted to load wall of nonexistent type {id}. Skipping.");
						continue;
					}

					loader.SetWall(x, y, id, wall.Health);
				}
				else if (loader.WallExists(x, y) && x != offset.X && y != offset.Y && y != maxY - 1 && !(x >= maxX - 2))
					loader.SetWall(x, y, 0, 0);
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
