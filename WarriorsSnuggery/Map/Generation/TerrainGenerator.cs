using System;
using System.Collections.Generic;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.Maps
{
	public class ActorGeneratorInfo
	{
		public readonly float Probability = 1f;

		public readonly float Health = 1f;
		public readonly string Type = string.Empty;
		public readonly byte Team = Actor.NeutralTeam;

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

					foreach (var a in info.SpawnActors)
					{
						var ran = random.NextDouble();
						if (ran <= a.Probability)
						{
							world.Add(ActorCreator.Create(world, a.Type, new CPos(1024 * x + random.Next(896) - 448, 1024 * y + random.Next(896) - 448, 0), a.Team, health: a.Health));
							break; // If an actor is already spawned, we don't want any other actor to spawn because they will probably overlap
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
		public readonly NoiseType NoiseType = NoiseType.CLOUDS;
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

	public sealed class TerrainGenerationType
	{
		public readonly int ID;

		public readonly NoiseType GenerationType;
		public readonly int Strength;
		public readonly float Scale;

		public readonly float Intensity;
		public readonly float Contrast;

		public readonly float EdgeNoise;

		public readonly int[] Terrain;
		public readonly bool SpawnPieces;
		public readonly int[] BorderTerrain;
		public readonly int Border;
		public readonly Dictionary<ActorType, int> SpawnActors;

		TerrainGenerationType(int id, NoiseType generationType, int strength, float scale, float intensity, float contrast, float edgeNoise, int[] terrain, bool spawnPieces, int[] borderTerrain, int border, Dictionary<ActorType, int> spawnActors)
		{
			ID = id;
			GenerationType = generationType;
			Strength = strength;
			Scale = scale;
			Intensity = intensity;
			Contrast = contrast;
			EdgeNoise = edgeNoise;
			Terrain = terrain;
			SpawnPieces = spawnPieces;
			BorderTerrain = borderTerrain;
			Border = border;
			SpawnActors = spawnActors;
		}

		public static TerrainGenerationType Empty()
		{
			return new TerrainGenerationType(0, NoiseType.NONE, 1, 1f, 1f, 1f, 0f, new[] { 0 }, true, new int[] { }, 0, new Dictionary<ActorType, int>());
		}

		public static TerrainGenerationType GetType(int id, MiniTextNode[] nodes)
		{
			var noise = NoiseType.NONE;
			var strength = 8;
			var scale = 2f;
			var intensity = 0f;
			var contrast = 1f;
			var edgeNoise = 0f;
			var terrainTypes = new int[0];
			var spawnPieces = true;
			var borderTerrain = new int[0];
			var border = 0;
			var spawnActorBlob = new Dictionary<ActorType, int>();

			foreach (var generation in nodes)
			{
				switch (generation.Key)
				{
					case "Type":
						noise = generation.Convert<NoiseType>();

						foreach (var noiseChild in generation.Children)
						{
							switch (noiseChild.Key)
							{
								case "Strength":
									strength = noiseChild.Convert<int>();
									break;
								case "Scale":
									scale = noiseChild.Convert<float>();
									break;
								case "Contrast":
									contrast = noiseChild.Convert<float>();
									break;
								case "Intensity":
									intensity = noiseChild.Convert<float>();
									break;
							}
						}
						break;
					case "Terrain":
						terrainTypes = generation.Convert<int[]>();

						break;
					case "Border":
						border = generation.Convert<int>();

						borderTerrain = generation.Children.Find(n => n.Key == "Terrain").Convert<int[]>();

						break;
					case "EdgeNoise":
						edgeNoise = generation.Convert<float>();

						break;
					case "SpawnPieces":
						spawnPieces = generation.Convert<bool>();

						break;
					case "SpawnActor":
						var type = ActorCreator.GetType(generation.Value);
						var probability = 50;

						probability = generation.Children.Find(n => n.Key == "Probability").Convert<int>();

						spawnActorBlob.Add(type, probability);
						break;
				}
			}
			return new TerrainGenerationType(id, noise, strength, scale, intensity, contrast, edgeNoise, terrainTypes, spawnPieces, borderTerrain, border, spawnActorBlob);
		}
	}
}