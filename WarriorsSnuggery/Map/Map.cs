using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WarriorsSnuggery.Maps;

namespace WarriorsSnuggery
{
	public enum NoiseType
	{
		NONE,
		NOISE,
		CLOUDS,
		MAZE
	}

	public class Map
	{
		readonly World world;
		readonly Random random;

		public readonly MapType Type;
		public readonly int Seed;

		// Map Information
		public MPos Bounds { get; private set; }
		public MPos Center { get { return Bounds / new MPos(2, 2); } }
		public MPos DefaultEdgeDistance { get { return Bounds / new MPos(8, 8); } }
		public CPos PlayerSpawn;
		public MPos Exit;

		bool[,] ActorSpawnPositions; // TODO remove
		bool[,] Used;
		int[,] TerrainGenerationArray;

		int[,] TilesWithAssignedGenerator;

		public Map(World world, MapType type, int seed, int level, int difficulty)
		{
			this.world = world;

			PlayerSpawn = type.SpawnPoint.ToCPos();
			Type = type;
			Seed = seed;
			random = new Random(seed);

			Bounds = type.CustomSize != MPos.Zero ? type.CustomSize : MapUtils.RandomMapBounds(random, difficulty, level, MapUtils.MinimumMapBounds, MapUtils.MaximumMapBounds);
		}

		public void Load()
		{
			Camera.SetBounds(Bounds);
			world.TerrainLayer.SetMapDimensions(Bounds);
			world.WallLayer.SetMapSize(Bounds);
			world.PhysicsLayer.SetMapDimensions(Bounds);
			world.ShroudLayer.SetMapDimensions(Bounds, Settings.MaxTeams, Type.DefaultType == GameType.MAINMENU || Type.DefaultType == GameType.MENU || Type.DefaultType == GameType.EDITOR || Type.DefaultType == GameType.TUTORIAL);

			VisibilitySolver.SetMapDimensions(Bounds, world.ShroudLayer);

			createGroundBase();

			Used = new bool[Bounds.X, Bounds.Y];
			TilesWithAssignedGenerator = new int[Bounds.X, Bounds.Y];
			ActorSpawnPositions = new bool[Bounds.X, Bounds.Y];

			// Important Parts
			if(!string.IsNullOrEmpty(Type.OverridePiece))
			{
				var input = RuleReader.Read(!Type.FromSave ? FileExplorer.FindPath(FileExplorer.Maps, Type.OverridePiece, ".yaml") : FileExplorer.Saves, Type.OverridePiece + ".yaml");

				LoadPiece(input.ToArray(), MPos.Zero, 100, true);
			}

			// mark tiles that don't allow placing pieces
			for (int x = 0; x < Bounds.X; x++)
			{
				for (int y = 0; y < Bounds.Y; y++)
				{
					var terrain = TerrainGenerationArray[x, y];
					if ((terrain == 0 && !Type.BaseTerrainGeneration.SpawnPieces) || (terrain != 0 && !Type.TerrainGeneration[terrain - 1].SpawnPieces))
						Used[x, y] = true;
				}
			}

			foreach (var info in Type.GeneratorInfos)
			{
				var generator = info.GetGenerator(random, this, world);
				generator.Generate();
			}

			for (int x = 0; x < Bounds.X; x++)
			{
				for (int y = 0; y < Bounds.Y; y++)
				{
					if (ActorSpawnPositions[x, y])
						continue;

					var gen = TerrainGenerationArray[x, y];
					var actors = gen == 0 ? Type.BaseTerrainGeneration.SpawnActors : Type.TerrainGeneration[gen - 1].SpawnActors;
					foreach (var a in actors)
					{
						var ran = random.NextDouble();
						if (ran <= a.Value / 100f)
							world.Add(ActorCreator.Create(world, a.Key, new CPos(1024 * x + random.Next(896) - 448, 1024 * y + random.Next(896) - 448, 0)));
					}
				}
			}
			MapPrinter.PrintMapGeneration("debug", TerrainGenerationArray, TilesWithAssignedGenerator, Type.GeneratorInfos.Length);
			// TODO dispose all unneeded elements from map generation (like the arrays)
		}

		public bool AcquireCell(MPos pos, int id)
		{
			if (TilesWithAssignedGenerator[pos.X, pos.Y] > id)
				return false;

			ActorSpawnPositions[pos.X, pos.Y] = true; // TODO
			TilesWithAssignedGenerator[pos.X, pos.Y] = id;
			return true;
		}

		void createGroundBase()
		{
			var random = new Random(Seed);

			float[] noise;
			switch (Type.BaseTerrainGeneration.GenerationType)
			{
				case NoiseType.CLOUDS:
					noise = Noise.GenerateClouds(Bounds, random, Type.BaseTerrainGeneration.Strength, Type.BaseTerrainGeneration.Scale);
					break;
				case NoiseType.NOISE:
					noise = Noise.GenerateNoise(Bounds, random, Type.BaseTerrainGeneration.Scale);
					break;
				case NoiseType.MAZE:
					noise = new float[Bounds.X * Bounds.Y];
					var maze = Maze.GenerateMaze(Bounds * new MPos(2, 2) + new MPos(1, 1), random, new MPos(1, 1), Type.BaseTerrainGeneration.Strength);

					for (int y = 0; y < Bounds.Y; y++)
					{
						for (int x = 0; x < Bounds.X; x++)
						{
							noise[y * Bounds.X + x] = maze[x, y].GetHashCode();
							if (maze[x, y])
								world.WallLayer.Set(WallCreator.Create(new WPos(x, y, 0), Type.Wall));
						}
					}

					for (int i = 0; i < Bounds.X; i++)
					{
						world.WallLayer.Set(WallCreator.Create(new WPos(1 + i * 2, 0, 0), Type.Wall));
						world.WallLayer.Set(WallCreator.Create(new WPos(0, i, 0), Type.Wall));
						world.WallLayer.Set(WallCreator.Create(new WPos(1 + i * 2, Bounds.Y, 0), Type.Wall));
						world.WallLayer.Set(WallCreator.Create(new WPos(Bounds.X * 2, i, 0), Type.Wall));
					}
					break;
				default:
					noise = new float[Bounds.X * Bounds.Y];
					break;
			}

			for (int y = 0; y < Bounds.Y; y++)
			{
				for (int x = 0; x < Bounds.X; x++)
				{
					var single = noise[y * Bounds.X + x];
					single += Type.BaseTerrainGeneration.Intensity;
					single = (single - 0.5f) * Type.BaseTerrainGeneration.Contrast + 0.5f;

					if (single > 1f)
						single = 1f;
					else if (single < 0f)
						single = 0f;

					var number = (int)Math.Floor(single * (Type.BaseTerrainGeneration.Terrain.Length - 1));
					world.TerrainLayer.Set(TerrainCreator.Create(world, new WPos(x, y, 0), Type.BaseTerrainGeneration.Terrain[number]));
				}
			}

			TerrainGenerationArray = new int[Bounds.X, Bounds.Y];
			foreach (var type in Type.TerrainGeneration)
			{
				createGround(type, ref TerrainGenerationArray);
			}
		}

		void createGround(TerrainGenerationType type, ref int[,] terrainGenerationArray)
		{
			var random = new Random(Seed);

			float[] noise = null;
			switch (type.GenerationType)
			{
				case NoiseType.CLOUDS:
					noise = Noise.GenerateClouds(Bounds, random, type.Strength, type.Scale);
					break;
				case NoiseType.NOISE:
					noise = Noise.GenerateNoise(Bounds, random, type.Scale);
					break;
				case NoiseType.MAZE:
					noise = new float[Bounds.X * Bounds.Y];
					var maze = Maze.GenerateMaze(Bounds * new MPos(2, 2) + new MPos(1, 1), random, new MPos(1, 1), Type.BaseTerrainGeneration.Strength);

					for (int y = 0; y < Bounds.Y; y++)
					{
						for (int x = 0; x < Bounds.X; x++)
						{
							noise[y * Bounds.X + x] = maze[x, y].GetHashCode();
						}
					}
					break;
			}

			for (int x = 0; x < Bounds.X; x++)
			{
				for (int y = 0; y < Bounds.Y; y++)
				{
					// Intensity and contrast
					var single = noise[y * Bounds.X + x];
					single += type.Intensity;
					single = (single - 0.5f) * type.Contrast + 0.5f;

					// Fit to area of 0 to 1.
					if (single > 1f) single = 1f;
					if (single < 0f) single = 0f;

					// If less than half, don't change terrain
					if (single < (float)random.NextDouble() * type.EdgeNoise + (1 - type.EdgeNoise) * 0.5f)
						continue;

					terrainGenerationArray[x, y] = type.ID;
					var number = (int)Math.Floor(single * (type.Terrain.Length - 1));
					world.TerrainLayer.Set(TerrainCreator.Create(world, new WPos(x, y, 0), type.Terrain[number]));

					if (type.Border > 0)
					{
						for (int by = 0; by < type.Border * 2 + 1; by++)
						{
							for (int bx = 0; bx < type.Border * 2 + 1; bx++)
							{
								var p = new MPos(x + by - type.Border, y + bx - type.Border);

								if (p.X < 0 || p.Y < 0)
									continue;
								if (p.X >= Bounds.X || p.Y >= Bounds.Y)
									continue;

								if (terrainGenerationArray[p.X, p.Y] != type.ID)
								{
									terrainGenerationArray[p.X, p.Y] = type.ID;
									world.TerrainLayer.Set(TerrainCreator.Create(world, new WPos(p.X, p.Y, 0), type.BorderTerrain[0]));
								}
							}
						}
					}
				}
			}
		}

		public bool LoadPiece(MiniTextNode[] nodes, MPos position, int ID, bool important = false, bool playerSpawn = false)
		{
			return GeneratePiece(Piece.LoadPiece(nodes), position, ID, important, playerSpawn);
		}

		public bool GeneratePiece(Piece piece, MPos position, int ID, bool important = false, bool playerSpawn = false)
		{
			if (!piece.IsInMap(position, Bounds))
			{
				Log.WriteDebug(string.Format("Piece '{0}' at Position '{1}' could not be created because it overlaps to the world's edge.", piece.Name, position));
				return false;
			}

			if (!important)
			{
				for (int x = position.X; x < (piece.Size.X + position.X); x++)
				{
					for (int y = position.Y; y < (piece.Size.Y + position.Y); y++)
					{
						if (Used[x, y] || !AcquireCell(new MPos(x, y), ID))
						{
							//Log.WriteDebug(string.Format("Tried to spawn piece '{0}' at position '{1}', but was already occupied.", piece.Name, position));
							return false;
						}
					}
				}
			}

			for (int x = position.X; x < (piece.Size.X + position.X); x++)
			{
				for (int y = position.Y; y < (piece.Size.Y + position.Y); y++)
				{
					Used[x, y] = true;
					ActorSpawnPositions[x, y] = true;
				}
			}

			piece.PlacePiece(position, world);

			if (playerSpawn)
				PlayerSpawn = new CPos(position.X * 1024 + piece.Size.X * 512, position.Y * 1024 + piece.Size.Y * 512, 0);

			return true;
		}

		public void Save(string name)
		{
			Save(FileExplorer.Maps, name);
		}

		public void Save(string directory, string name)
		{
			SaveFile(directory + @"maps/"+ name + ".yaml", name);
		}

		public void SaveFile(string file, string name)
		{
			using (var writer = new StreamWriter(file, false))
			{
				writer.WriteLine("Name=" + name);
				writer.WriteLine("Size=" + Bounds.X + "," + Bounds.Y);

				var terrain = "Terrain=";
				for (int y = 0; y < Bounds.Y; y++)
				{
					for (int x = 0; x < Bounds.X; x++)
					{
						terrain += world.TerrainLayer.Terrain[x, y].Type.ID + ",";
					}
				}

				terrain = terrain.Substring(0, terrain.Length - 1);
				writer.WriteLine(terrain);

				var walls = "Walls=";
				for (int y = 0; y < world.WallLayer.Size.Y - 1; y++)
				{
					for (int x = 0; x < world.WallLayer.Size.X - 1; x++)
					{
						walls += (world.WallLayer.Walls[x, y] == null ? -1 : world.WallLayer.Walls[x, y].Type.ID) + ",";
					}
				}

				walls = walls.Substring(0, walls.Length - 1);
				writer.WriteLine(walls);

				writer.WriteLine("Actors=");
				for (int i = 0; i < world.Actors.Count; i++)
				{
					var a = world.Actors[i];
					writer.WriteLine("\t" + i + "=" + a.Position.X + "," + a.Position.Y + "," + a.Position.Z);
					writer.WriteLine("\t\t" + "Type=" + ActorCreator.GetName(a.Type));
					if (a.Team != Objects.Actor.NeutralTeam)
						writer.WriteLine("\t\t" + "Team=" + a.Team);
					if (a.Health != null && a.Health.HP != a.Health.MaxHP)
						writer.WriteLine("\t\t" + "Health=" + a.Health.HPRelativeToMax);
					if (a.IsBot)
						writer.WriteLine("\t\t" + "IsBot=" + a.IsBot);
					if (a.IsPlayer)
						writer.WriteLine("\t\t" + "IsPlayer=" + a.IsPlayer);
				}

				writer.Flush();
				writer.Close();
			}
		}
	}
}
