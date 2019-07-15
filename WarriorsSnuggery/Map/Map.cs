using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WarriorsSnuggery.Maps;

namespace WarriorsSnuggery
{
	public enum GenerationType
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
		public MPos Exit { get; private set; }

		bool[,] ActorSpawnPositions;
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

			Bounds = type.CustomSize != MPos.Zero ? type.CustomSize : /*MapUtils.RandomMapBounds(random, difficulty, level, MapUtils.MinimumMapBounds, MapUtils.MaximumMapBounds)*/MapUtils.MaximumMapBounds;
		}

		public void Load()
		{
			Camera.SetBounds(Bounds);

			createGroundBase();

			Used = new bool[Bounds.X, Bounds.Y];
			TilesWithAssignedGenerator = new int[Bounds.X, Bounds.Y];
			ActorSpawnPositions = new bool[Bounds.X, Bounds.Y];

			// Important Parts
			foreach (var node in Type.ImportantParts)
			{
				var input = RuleReader.Read(!Type.FromSave ? FileExplorer.FindPath(FileExplorer.Maps, node.Key, ".yaml") : FileExplorer.Saves, node.Key + ".yaml");

				LoadPiece(input.ToArray(), node.Value, 100, true);
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

			// Entrances
			createEntry(random);

			// Exits
			if (world.Game.Mode == GameMode.FIND_EXIT && Type.Exits.Any())
			{
				createExit(random);
			}

			foreach (var info in Type.GeneratorInfos)
			{
				var generator = info.GetGenerator(random, this, world);
				generator.Generate();
			}

			//for (int y = 0; y < Size.Y; y++)
			//{
			//	for (int x = 0; x < Size.X; x++)
			//	{
			//		if (ActorSpawnPositions[x, y])
			//			continue;

			//		var gen = TerrainGenerationArray[x, y];
			//		var actors = gen == 0 ? Type.BaseTerrainGeneration.SpawnActors : Type.TerrainGeneration[gen - 1].SpawnActors;
			//		foreach (var a in actors)
			//		{
			//			var ran = random.Next(100);
			//			if (ran <= a.Value)
			//				world.Add(ActorCreator.Create(world, a.Key, new CPos(1024 * x + random.Next(896) - 448, 1024 * y + random.Next(896) - 448, 0)));
			//		}
			//	}
			//}
			MapPrinter.PrintMapGeneration("debug", 8, TerrainGenerationArray, Used);
			// TODO dispose all unneeded elements from map generation (like the arrays)
		}

		public bool AcquireCell(MPos pos, int id) // TODO
		{
			if (TilesWithAssignedGenerator[pos.X, pos.Y] > id)
				return false;

			TilesWithAssignedGenerator[pos.X, pos.Y] = id;
			return true;
		}

		void createEntry(Random random)
		{
			if (Type.Entrances.Any())
			{
				// We are estimating here that the entrance tile won't be larger than 8x8.

				var name = Type.RandomEntrance(random);
				var piece = RuleReader.Read(FileExplorer.FindPath(FileExplorer.Maps, name, ".yaml"), name + ".yaml");
				var size = pieceSize(piece);

				var spawnArea = Bounds - size;
				var half = spawnArea / new MPos(2, 2);
				var quarter = spawnArea / new MPos(4, 4);
				var pos = half + new MPos(random.Next(quarter.X) - quarter.X / 2, random.Next(quarter.Y) - quarter.Y / 2);

				LoadPiece(piece.ToArray(), pos, 100, true, true);
			}
		}

		void createExit(Random random)
		{
			// We are estimating here that the exit tile won't be larger than 8x8.
			var name = Type.RandomExit(random);
			var piece = RuleReader.Read(FileExplorer.FindPath(FileExplorer.Maps, name, ".yaml"), name + ".yaml");
			var size = pieceSize(piece);

			var spawnArea = Bounds - size;
			var pos = MPos.Zero;
			// Picking a random side, 0 = x, 1 = y, 2 = -x, 3 = -y;
			var side = (byte)random.Next(4);
			switch (side)
			{
				case 0:
					pos = new MPos(random.Next(2), random.Next(spawnArea.X));
					break;
				case 1:
					pos = new MPos(random.Next(spawnArea.Y), random.Next(2));
					break;
				case 2:
					pos = new MPos(spawnArea.X - random.Next(2), random.Next(spawnArea.X));
					break;
				case 3:
					pos = new MPos(random.Next(spawnArea.X), spawnArea.Y - random.Next(2));
					break;
			}

			while (!LoadPiece(piece.ToArray(), pos, 0, true))
			{
				spawnArea = Bounds - size;
				pos = MPos.Zero;
				// Picking a random side, 0 = x, 1 = y, 2 = -x, 3 = -y;
				side = (byte)random.Next(4);
				switch (side)
				{
					case 0:
						pos = new MPos(random.Next(2), random.Next(spawnArea.X));
						break;
					case 1:
						pos = new MPos(random.Next(spawnArea.Y), random.Next(2));
						break;
					case 2:
						pos = new MPos(spawnArea.X - random.Next(2), random.Next(spawnArea.X));
						break;
					case 3:
						pos = new MPos(random.Next(spawnArea.X), spawnArea.Y - random.Next(2));
						break;
				}
			}

			Exit = pos + size / new MPos(2, 2);
		}

		void createGroundBase()
		{
			world.TerrainLayer.SetMapDimensions(Bounds);
			world.WallLayer.SetMapSize(Bounds);
			world.PhysicsLayer.SetMapDimensions(Bounds);
			world.ShroudLayer.SetMapDimensions(Bounds, Settings.MaxTeams, Type.DefaultType == GameType.MAINMENU || Type.DefaultType == GameType.MENU || Type.DefaultType == GameType.EDITOR || Type.DefaultType == GameType.TUTORIAL);

			VisibilitySolver.SetMapDimensions(Bounds, world.ShroudLayer);

			var random = new Random(Seed);

			float[] noise;
			switch (Type.BaseTerrainGeneration.GenerationType)
			{
				case GenerationType.CLOUDS:
					noise = Noise.GenerateClouds(Bounds, random, Type.BaseTerrainGeneration.Strength, Type.BaseTerrainGeneration.Scale);
					break;
				case GenerationType.NOISE:
					noise = Noise.GenerateNoise(Bounds, random, Type.BaseTerrainGeneration.Scale);
					break;
				case GenerationType.MAZE:
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
				MapPrinter.PrintMapGeneration("debug", type.ID, TerrainGenerationArray);
			}
		}

		void createGround(TerrainGenerationType type, ref int[,] terrainGenerationArray)
		{
			var random = new Random(Seed);

			float[] noise = null;
			switch (type.GenerationType)
			{
				case GenerationType.CLOUDS:
					noise = Noise.GenerateClouds(Bounds, random, type.Strength, type.Scale);
					break;
				case GenerationType.NOISE:
					noise = Noise.GenerateNoise(Bounds, random, type.Scale);
					break;
				case GenerationType.MAZE:
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

			for (int y = 0; y < Bounds.Y; y++)
			{
				for (int x = 0; x < Bounds.X; x++)
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
			var piece = Piece.LoadPiece(nodes);

			if (!piece.IsInMap(position, Bounds))
			{
				Log.WriteDebug(string.Format("Piece '{0}' at Position '{1}' could not be created because it overlaps to the world's edge.", piece.Name, position));
				return false;
			}

			if (!important)
			{
				for (int y = position.Y; y < (piece.Size.Y + position.Y); y++)
				{
					for (int x = position.X; x < (piece.Size.X + position.X); x++)
					{
						if (Used[x, y] || !AcquireCell(new MPos(x, y), ID))
						{
							Log.WriteDebug(string.Format("Piece '{0}' at Position '{1}': Position is already occupied.", piece.Name, position));
							return false;
						}
					}
				}
			}

			for (int y = position.Y; y < (piece.Size.Y + position.Y); y++)
			{
				for (int x = position.X; x < (piece.Size.X + position.X); x++)
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

		MPos pieceSize(List<MiniTextNode> nodes)
		{
			return nodes.First(n => n.Key == "Size").Convert<MPos>();
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

				writer.Flush();
				writer.Close();
			}
		}
	}
}
