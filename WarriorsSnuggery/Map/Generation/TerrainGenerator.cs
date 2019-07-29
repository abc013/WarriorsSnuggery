using System;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.Maps
{
	public class ActorGeneratorInfo
	{
		public readonly float Probability = 1f;

		public readonly float Health = 1f;
		public readonly string Type = string.Empty;
		public readonly byte Team = Actor.NeutralTeam;
		public readonly bool IsBot = false;

		public ActorGeneratorInfo(MiniTextNode[] nodes)
		{
			Loader.PartLoader.SetValues(this, nodes);
		}
	}

	public class TerrainGenerator : MapGenerator
	{
		readonly TerrainGeneratorInfo info;
		public TerrainGenerator(Random random, Map map, World world, TerrainGeneratorInfo info) : base(random, map, world)
		{
			this.info = info;
		}

		public override void Generate()
		{
			float[] noise = null;
			switch (info.NoiseType)
			{
				case NoiseType.CLOUDS:
					noise = Noise.GenerateClouds(map.Bounds, random, info.Strength, info.Scale);
					break;
				case NoiseType.NOISE:
					noise = Noise.GenerateNoise(map.Bounds, random, info.Scale);
					break;
				case NoiseType.MAZE:
					noise = new float[map.Bounds.X * map.Bounds.Y];
					var maze = Maze.GenerateMaze(map.Bounds * new MPos(2, 2) + new MPos(1, 1), random, new MPos(1, 1), info.Strength);

					for (int x = 0; x < map.Bounds.X; x++)
					{
						for (int y = 0; y < map.Bounds.Y; y++)
						{
							noise[x * map.Bounds.Y + y] = maze[x, y].GetHashCode();
						}
					}
					break;
				case NoiseType.NONE:
					noise = new float[map.Bounds.X * map.Bounds.Y];

					if (info.Strength > 0)
					{
						for (int x = 0; x < map.Bounds.X; x++)
						{
							for (int y = 0; y < map.Bounds.Y; y++)
							{
								noise[x * map.Bounds.Y + y] = 1;
							}
						}
					}
					break;
			}

			for (int x = 0; x < map.Bounds.X; x++)
			{
				for (int y = 0; y < map.Bounds.Y; y++)
				{
					// Intensity and contrast
					var single = noise[y * map.Bounds.X + x];
					single += info.Intensity;
					single = (single - 0.5f) * info.Contrast + 0.5f;

					// Fit to area of 0 to 1.
					if (single > 1f) single = 1f;
					if (single < 0f) single = 0f;

					// If less than half, don't change terrain
					if (single < (float)random.NextDouble() * info.EdgeNoise + (1 - info.EdgeNoise) * 0.5f)
						continue;

					if (!map.AcquireCell(new MPos(x, y), info.ID))
						continue;

					dirtyCells[x, y] = true;
					//terrainGenerationArray[x, y] = info.ID;
					var number = (int)Math.Floor(single * (info.Terrain.Length - 1));
					world.TerrainLayer.Set(TerrainCreator.Create(world, new WPos(x, y, 0), info.Terrain[number]));

					if (info.SpawnActors != null)
					{
						foreach (var a in info.SpawnActors)
						{
							var ran = random.NextDouble();
							if (ran <= a.Probability)
							{
								world.Add(ActorCreator.Create(world, a.Type, new CPos(1024 * x + random.Next(896) - 448, 1024 * y + random.Next(896) - 448, 0), a.Team, a.IsBot, health: a.Health));
								break; // If an actor is already spawned, we don't want any other actor to spawn because they will probably overlap
							}
						}
					}

					if (info.Border > 0)
					{
						for (int by = 0; by < info.Border * 2 + 1; by++)
						{
							for (int bx = 0; bx < info.Border * 2 + 1; bx++)
							{
								var p = new MPos(x + by - info.Border, y + bx - info.Border);

								if (p.X < 0 || p.Y < 0)
									continue;
								if (p.X >= map.Bounds.X || p.Y >= map.Bounds.Y)
									continue;

								if (!dirtyCells[p.X, p.Y] && map.AcquireCell(p, info.ID))
								{
									world.TerrainLayer.Set(TerrainCreator.Create(world, new WPos(p.X, p.Y, 0), info.BorderTerrain[0]));
								}
							}
						}
					}
				}
			}
		}

		protected override void MarkDirty()
		{
			throw new NotImplementedException();
		}

		protected override void DrawDirty()
		{
			throw new NotImplementedException();
		}

		protected override void ClearDirty()
		{
			throw new NotImplementedException();
		}
	}

	public class TerrainGeneratorInfo : MapGeneratorInfo
	{
		[Desc("Unique ID for the generator.")]
		public readonly new int ID;

		[Desc("Type of noise to use.")]
		public readonly NoiseType NoiseType = NoiseType.NONE;
		[Desc("Strength of the noise [Clouds].", "count of additional pathways [Maze].")]
		public readonly int Strength = 4;
		[Desc("Scale of the noise [Noise, Clouds].")]
		public readonly float Scale = 1f;

		[Desc("Intensity parameter.")]
		public readonly float Intensity = 0f;
		[Desc("Contrast parameter.")]
		public readonly float Contrast = 1f;

		[Desc("Noise used for the edge.")]
		public readonly float EdgeNoise = 0f;

		[Desc("Terrain to use.")]
		public readonly int[] Terrain = new int[] { 0 };
		[Desc("Allows spawning of pieces.")]
		public readonly bool SpawnPieces = true;
		[Desc("Information about the actors to be spawned on that terrain.")]
		public readonly ActorGeneratorInfo[] SpawnActors;

		[Desc("Border thickness.")]
		public readonly int Border = 0;
		[Desc("Terrain to use for borders.")]
		public readonly int[] BorderTerrain = new int[0];

		public TerrainGeneratorInfo(int id, MiniTextNode[] nodes) : base(id)
		{
			ID = id;
			Loader.PartLoader.SetValues(this, nodes);
		}

		public override MapGenerator GetGenerator(Random random, Map map, World world)
		{
			return new TerrainGenerator(random, map, world, this);
		}
	}
}