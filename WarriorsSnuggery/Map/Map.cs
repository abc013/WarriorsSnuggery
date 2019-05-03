using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
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

		public readonly MapType Type;
		public readonly int Seed;

		// Map Information
		public MPos Size { get; private set; }
		public MPos Mid { get { return Size / new MPos(2, 2); } }
		public MPos DefaultEdgeDistance { get { return Size / new MPos(8, 8); } }
		public CPos PlayerStart;

		bool[,] ActorSpawnPositions;
		bool[,] Used;
		int[,] TerrainGenerationArray;

		public Map(World world, MapType type, int seed, int level, int difficulty)
		{
			this.world = world;

			PlayerStart = type.SpawnPoint.ToCPos();
			Type = type;
			Seed = seed;

			generateMapSize(seed, level, difficulty);
		}

		void generateMapSize(int seed, int level, int difficulty)
		{
			float calc1 = 10 * ((level + 1) + (1 - 1f/(difficulty + 1)));
			float calc2 = (float) new Random(seed).NextDouble() + 0.1f;

			Size = new MPos((int) (calc1 * 2 * calc2), (int) (calc1 * 2 * calc2));

			if (Size.X < 16)
				Size = new MPos(16,16);

			if (Size.X > 64)
				Size = new MPos(64, 64);
		}

		public void Load()
		{
			if (Type.CustomSize != MPos.Zero)
				Size = Type.CustomSize;

			Camera.SetBounds(Size.ToCPos());
			createGroundBase();
			Used = new bool[Size.X,Size.Y];
			ActorSpawnPositions = new bool[Size.X, Size.Y];

			var random = new Random(Seed);

			// Important Parts
			foreach(var node in Type.ImportantParts)
			{
				var input = RuleReader.Read(FileExplorer.Maps + node.Key + @"\map.yaml");

				loadPiece(input, node.Value, true);
			}

			// Entrances
			if (Type.Entrances.Any())
			{
				// We are estimating here that the entrance tile won't be larger than 8x8.
				var piece = RuleReader.Read(FileExplorer.Maps + Type.RandomEntrance(random) + @"\map.yaml");
				var size = pieceSize(piece);

				var spawnArea = Size - size;
				var half = spawnArea / new MPos(2, 2);
				var quarter = spawnArea / new MPos(4, 4);
				var pos = half + new MPos(random.Next(quarter.X) - quarter.X / 2, random.Next(quarter.Y) - quarter.Y / 2);

				loadPiece(piece, pos, playerSpawn: true);
			}

			for (int x = 0; x < Size.X; x++)
			{
				for (int y = 0; y < Size.Y; y++)
				{
					var terrain = TerrainGenerationArray[x, y];
					if ((terrain == 0 && !Type.BaseTerrainGeneration.SpawnPieces) || (terrain != 0 && !Type.TerrainGeneration[terrain - 1].SpawnPieces))
						Used[x, y] = true;
				}
			}

			// Exits
			if (Type.Exits.Any())
			{
				// We are estimating here that the exit tile won't be larger than 8x8.
				var piece = RuleReader.Read(FileExplorer.Maps + Type.RandomExit(random) + @"\map.yaml");
				var size = pieceSize(piece);

				var spawnArea = Size - size;
				var pos = MPos.Zero;
				// Picking a random side, 0 = x, 1 = y, 2 = -x, 3 = -y;
				var side = (byte) random.Next(4);
				switch(side)
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

				while(!loadPiece(piece, pos))
				{
					piece = RuleReader.Read(FileExplorer.Maps + Type.RandomExit(random) + @"\map.yaml");
					size = pieceSize(piece);

					spawnArea = Size - size;
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
			}

			// Normal Parts
			if (Type.Parts.Any())
			{
				// We are using a higher value because we don't know how much of the buildings apparently are going to spawn.
				var piecesToUse = (int) Math.Pow((Size.X + Size.Y) / 10, 2) * 2;

				for (int i = 0; i < piecesToUse; i++)
				{
					var piece = RuleReader.Read(FileExplorer.Maps + Type.RandomPiece(random) + @"\map.yaml");
					var size = pieceSize(piece);

					var spawnArea = Size - size;


					loadPiece(piece, new MPos(random.Next(spawnArea.X), random.Next(spawnArea.Y)));
				}
			}

			for (int y = 0; y < Size.Y; y++)
			{
				for (int x = 0; x < Size.X; x++)
				{
					if (ActorSpawnPositions[x, y])
						continue;

					var gen = TerrainGenerationArray[x, y];
					var actors = gen == 0 ? Type.BaseTerrainGeneration.SpawnActors : Type.TerrainGeneration[gen - 1].SpawnActors;
					foreach (var a in actors)
					{
						var ran = random.Next(100);
						if (ran <= a.Value)
							world.Add(ActorCreator.Create(world, a.Key, new CPos(1024 * x + random.Next(1024) - 512, 1024 * y + random.Next(1024) - 512, 0)));
					}
				}
			}
		}

		void createGroundBase()
		{
			world.TerrainLayer.SetMapSize(Size);
			world.WallLayer.SetMapSize(Size);
			world.PhysicsLayer.SetMapSize(Size);

			var random = new Random(Seed);

			float[] noise = null;
			switch (Type.BaseTerrainGeneration.GenerationType)
			{
				case GenerationType.CLOUDS:
					noise = Noise.GenerateClouds(Size, random, Type.BaseTerrainGeneration.Strength, Type.BaseTerrainGeneration.Scale);
					break;
				case GenerationType.NOISE:
					noise = Noise.GenerateNoise(Size, random, Type.BaseTerrainGeneration.Scale);
					break;
				case GenerationType.MAZE:
					noise = new float[Size.X * Size.Y];
					var maze = Maze.GenerateMaze(Size * new MPos(2, 2) + new MPos(1, 1), random, new MPos(1, 1), Type.BaseTerrainGeneration.Strength);

					for (int y = 0; y < Size.Y; y++)
					{
						for (int x = 0; x < Size.X; x++)
						{
							noise[y * Size.X + x] = maze[x, y].GetHashCode();
							if (maze[x, y])
								world.WallLayer.Set(WallCreator.Create(new WPos(x, y, 0), Type.Wall));
						}
					}

					for (int i = 0; i < Size.X; i++)
					{
						world.WallLayer.Set(WallCreator.Create(new WPos(1 + i * 2, 0, 0), Type.Wall));
						world.WallLayer.Set(WallCreator.Create(new WPos(0, i, 0), Type.Wall));
						world.WallLayer.Set(WallCreator.Create(new WPos(1 + i * 2, Size.Y, 0), Type.Wall));
						world.WallLayer.Set(WallCreator.Create(new WPos(Size.X * 2, i, 0), Type.Wall));
					}
					break;
				default:
					noise = new float[Size.X * Size.Y];
					break;
			}

			for (int y = 0; y < Size.Y; y++)
			{
				for (int x = 0; x < Size.X; x++)
				{
					var single = noise[y * Size.X + x];
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

			TerrainGenerationArray = new int[Size.X, Size.Y];
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
					noise = Noise.GenerateClouds(Size, random, type.Strength, type.Scale);
					break;
				case GenerationType.NOISE:
					noise = Noise.GenerateNoise(Size, random, type.Scale);
					break;
				case GenerationType.MAZE:
					noise = new float[Size.X * Size.Y];
					var maze = Maze.GenerateMaze(Size * new MPos(2, 2) + new MPos(1, 1), random, new MPos(1, 1), Type.BaseTerrainGeneration.Strength);

					for (int y = 0; y < Size.Y; y++)
					{
						for (int x = 0; x < Size.X; x++)
						{
							noise[y * Size.X + x] = maze[x, y].GetHashCode();
						}
					}
					break;
			}

			for (int y = 0; y < Size.Y; y++)
			{
				for (int x = 0; x < Size.X; x++)
				{
					// Intensity and contrast
					var single = noise[y * Size.X + x];
					single += type.Intensity;
					single = (single - 0.5f) * type.Contrast + 0.5f;

					// Fit to area of 0 to 1.
					if (single > 1f) single = 1f;
					if (single < 0f) single = 0f;

					// If less than half, don't change terrain
					if (single < 0.5f) // TODO add value and noise (for grainy things)
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
								if (p.X >= Size.X || p.Y >= Size.Y)
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

		bool loadPiece(List<MiniTextNode> nodes, MPos position, bool important = false, bool playerSpawn = false) //TODO move into single class named PieceLoader, create Piece class
		{
			MPos size = MPos.Zero;

			string[] groundData = null;
			string[] wallData = new string[0];

			string name = "unnamed piece";
			List<MiniTextNode> actors = null;

			foreach(var rule in nodes)
			{
				switch(rule.Key)
				{
					case "Size":
						size = rule.ToMPos();

						break;
					case "Terrain":
						groundData = rule.ToArray();

						break;
					case "Walls":
						wallData = rule.ToArray();

						break;
					case "Name":
						name = rule.Value;

						break;
					case "Actors":
						actors = rule.Children;

						break;
				}
			}
			var endPosition = size + position;
			if (endPosition.X > Size.X || endPosition.Y > Size.Y)
			{
				Log.WriteDebug(string.Format("Piece '{0}' at Position '{1}' could not be created because it overlaps to the world's edge.", name, position));
				return false;
			}
			if (!important)
			{
				for (int y = position.Y; y < (size.Y + position.Y); y++)
				{
					for (int x = position.X; x < (size.X + position.X); x++)
					{
						if (Used[x, y])
						{
							Log.WriteDebug(string.Format("Piece '{0}' at Position '{1}': Position is already occupied.", name, position));
							return false;
						}
					}
				}
			}

			if (groundData.Length < size.X * size.Y)
				throw new YamlInvalidNodeException(string.Format(@"The count of given terrains ({0}) is not the same as the size ({1}) on the piece '{2}'", groundData.Length, Size.X * Size.Y, name));

			if (wallData.Length != 0 && wallData.Length < size.X * size.Y)
				throw new YamlInvalidNodeException(string.Format(@"The count of given walls ({0}) is not the same as the size ({1}) on the piece '{2}'", groundData.Length, Size.X * 2 * Size.Y, name));

			var terrain = new int[groundData.Length];
			for(int i = 0; i < groundData.Length; i++)
			{
				if (!int.TryParse(groundData[i], out terrain[i]))
					throw new YamlInvalidNodeException(string.Format(@"unable to load terrain-ID '{0}' in piece '{1}'.", terrain[i], name));
			}

			for(int y = position.Y; y < (size.Y + position.Y); y++)
			{
				for(int x = position.X; x < (size.X + position.X); x++)
				{
					Used[x,y] = true;
					ActorSpawnPositions[x, y] = true;
					world.TerrainLayer.Set(TerrainCreator.Create(world, new WPos(x,y,0), terrain[(y - position.Y) * size.X + (x - position.X)]));
				}
			}

			if (wallData.Length > 0)
			{
				var walls = new int[wallData.Length];
				for(int i = 0; i < wallData.Length; i++)
				{
					if (!int.TryParse(wallData[i], out walls[i]))
						throw new YamlInvalidNodeException(string.Format(@"unable to load wall-ID '{0}' in piece '{1}'.", terrain[i], name));
				}
				for(int y = position.Y; y < (size.Y + position.Y); y++)
				{
					for(int x = position.X * 2; x < (size.X + position.X) * 2; x++)
					{
						if (walls[(y - position.Y) * size.X*2 + (x - position.X * 2)] >= 0)
							world.WallLayer.Set(WallCreator.Create(new WPos(x,y,0), walls[(y - position.Y) * size.X*2 + (x - position.X * 2)]));
						else
							world.WallLayer.Remove(new MPos(x,y));
					}
				}
			}

			if (actors != null)
			{
				foreach(var actor in actors)
				{
					try
					{
						var split = actor.Key.Split(';');
						var type = split[0];
						var team = split.Length > 1 ? int.Parse(split[1]) : 0;
						var bot = split.Length > 1 ? split[2].Equals("true") : false;
						world.Add(ActorCreator.Create(world, type, actor.ToCPos() + position.ToCPos(), team, bot));
					}
					catch (Exception e)
					{
						throw new YamlInvalidNodeException(string.Format("Could not create Actor from '{0}' on piece '{1}'", actor.Key + "=" + actor.Value, name), e);
					}
				}
			}

			if (playerSpawn)
				PlayerStart = new CPos(position.X * 1024 + size.X * 512, position.Y * 1024 + size.Y * 512, 0);

			return true;
		}

		MPos pieceSize(List<MiniTextNode> nodes)
		{
			foreach(var node in nodes)
			{
				if (node.Key == "Size")
					return node.ToMPos();
			}

			throw new YamlMissingNodeException("Piece", "Size");
		}

		public void Save(string name)
		{
			using(var writer = new StreamWriter(FileExplorer.Maps + name + @"\map.yaml", false))
			{
				writer.WriteLine("Name=" + name);
				writer.WriteLine("Size=" + Size.X + "," + Size.Y);

				var terrain = "Terrain=";
				for(int y = 0; y < world.TerrainLayer.Size.Y; y++)
				{
					for(int x = 0; x < world.TerrainLayer.Size.X; x++)
					{
						terrain += world.TerrainLayer.Terrain[x,y].Type.ID + ",";
					}
				}

				terrain = terrain.Substring(0, terrain.Length - 1);
				writer.WriteLine(terrain);

				writer.WriteLine("Actors=");
				foreach(var a in world.Actors)
					writer.WriteLine("\t" + ActorCreator.GetName(a.Type) + ";" + a.Team + ";" + (a.IsBot + "").ToLower() + "=" + a.Position.X + "," + a.Position.Y + "," + a.Position.Z);

				var walls = "Walls=";
				for(int y = 0; y < world.WallLayer.Size.Y - 1; y++)
				{
					for(int x = 0; x < world.WallLayer.Size.X - 1; x++)
					{
						walls += (world.WallLayer.Walls[x,y] == null ? -1 : world.WallLayer.Walls[x,y].Type.ID) + ",";
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
