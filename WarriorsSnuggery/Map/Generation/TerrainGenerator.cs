﻿using System;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.Maps
{
	[Desc("Generator used for generating random flocks of terrain or actors on the map.")]
	public class TerrainGeneratorInfo : MapGeneratorInfo
	{
		[Desc("Unique ID for the generator.")]
		public readonly new int ID;

		[Desc("ID for the noisemap.", "Set to a negative value to not use one.")]
		public readonly int NoiseMapID = -1;

		[Desc("Range steps used for the ProbabilitySteps.", "Defines at which range the probability points are defined.", "The range goes from 0.0 to 1.0.")]
		public readonly float[] RangeSteps = new[] { 1f };
		[Desc("Terrain override percentage at each range step.")]
		public readonly float[] ProbabilitySteps = new[] { 1f };

		[Desc("Terrain to use.")]
		public readonly ushort[] Terrain = new ushort[] { 0 };
		[Desc("Allows spawning of pieces.")]
		public readonly bool SpawnPieces = true;
		[Desc("Information about the actors to be spawned on that terrain.")]
		public readonly ActorProbabilityInfo[] SpawnActors;

		[Desc("Border thickness.")]
		public readonly int Border = 0;
		[Desc("Terrain to use for borders.")]
		public readonly ushort[] BorderTerrain = new ushort[0];

		public TerrainGeneratorInfo(int id, MiniTextNode[] nodes) : base(id)
		{
			ID = id;
			Loader.PartLoader.SetValues(this, nodes);

			if (RangeSteps.Length != ProbabilitySteps.Length)
				throw new YamlInvalidNodeException($"Range step length ({RangeSteps.Length}) does not match with given provabability values ({ProbabilitySteps.Length}).");
		}

		public override MapGenerator GetGenerator(Random random, Map map, World world)
		{
			return new TerrainGenerator(random, map, world, this);
		}
	}

	[Desc("Information about objects that can be spawned with the TerrainGenerator.")]
	public class ActorProbabilityInfo
	{
		[Desc("Type of the actor.")]
		public readonly ActorType Type;

		[Desc("Probability of spawning an actor on a field.")]
		public readonly float Probability = 1f;

		[Desc("Health in percentage, if the Healthpart is given in the actor's definitions.")]
		public readonly float Health = 1f;
		[Desc("Team of the actor.")]
		public readonly byte Team = Actor.NeutralTeam;
		[Desc("Determines whether the actor spawned is a bot.")]
		public readonly bool IsBot = false;

		public ActorProbabilityInfo(MiniTextNode[] nodes)
		{
			Loader.PartLoader.SetValues(this, nodes);

			if (Type == null)
				throw new YamlMissingNodeException("SpawnActors", "Type");
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
			var noise = GeneratorUtils.GetNoise(map, info.NoiseMapID);

			for (int x = 0; x < map.Bounds.X; x++)
			{
				for (int y = 0; y < map.Bounds.Y; y++)
				{
					var value = noise[y * map.Bounds.X + x];
					var randomValue = random.NextDouble();
					var limit = GeneratorUtils.Multiplier(info.ProbabilitySteps, info.RangeSteps, value);

					if (randomValue > limit)
						continue;

					if (!map.AcquireCell(new MPos(x, y), info.ID))
						continue;

					dirtyCells[x, y] = true;
					//terrainGenerationArray[x, y] = info.ID;
					var number = (int)Math.Floor(value * (info.Terrain.Length - 1));
					world.TerrainLayer.Set(TerrainCreator.Create(world, new MPos(x, y), info.Terrain[number]));

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
									world.TerrainLayer.Set(TerrainCreator.Create(world, new MPos(p.X, p.Y), info.BorderTerrain[0]));
							}
						}
					}
				}
			}

			MapPrinter.PrintGeneratorMap(map.Bounds, noise, dirtyCells, info.ID);
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
}