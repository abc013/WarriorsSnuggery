﻿using System;
using System.Collections.Generic;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.Maps.Generators
{
	[Desc("Generator used for generating random flocks of terrain or actors on the map.")]
	public class TerrainGeneratorInfo : IMapGeneratorInfo
	{
		public int ID { get; private set; }

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

		public TerrainGeneratorInfo(int id, List<TextNode> nodes)
		{
			ID = id;
			TypeLoader.SetValues(this, nodes);

			if (RangeSteps.Length != ProbabilitySteps.Length)
				throw new InvalidNodeException($"Range step length ({RangeSteps.Length}) does not match with given provabability values ({ProbabilitySteps.Length}).");
		}

		public MapGenerator GetGenerator(Random random, MapLoader loader)
		{
			return new TerrainGenerator(random, loader, this);
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

		public ActorProbabilityInfo(List<TextNode> nodes)
		{
			TypeLoader.SetValues(this, nodes);

			if (Type == null)
				throw new MissingNodeException("SpawnActors", "Type");
		}
	}

	public class TerrainGenerator : MapGenerator
	{
		readonly TerrainGeneratorInfo info;

		public TerrainGenerator(Random random, MapLoader loader, TerrainGeneratorInfo info) : base(random, loader)
		{
			this.info = info;
		}

		public override void Generate()
		{
			var noise = GeneratorUtils.GetNoise(Loader, info.NoiseMapID);

			for (int x = 0; x < Bounds.X; x++)
			{
				for (int y = 0; y < Bounds.Y; y++)
				{
					var value = noise[x, y];
					var randomValue = Random.NextDouble();
					var limit = GeneratorUtils.Multiplier(info.ProbabilitySteps, info.RangeSteps, value);

					if (randomValue > limit)
						continue;

					if (!Loader.AcquireCell(new MPos(x, y), info.ID, denyPatrols: false))
						continue;

					UsedCells[x, y] = true;

					var number = (int)Math.Floor(value * (info.Terrain.Length - 1));
					Loader.SetTerrain(x, y, info.Terrain[number]);

					if (info.SpawnActors != null)
					{
						foreach (var a in info.SpawnActors)
						{
							if (Random.NextDouble() <= a.Probability)
							{
								Loader.AddActor(new CPos(1024 * x + Random.Next(896) - 448, 1024 * y + Random.Next(896) - 448, 0), a);
								break; // If an actor is already spawned, we don't want any other actor to spawn because they will probably overlap
							}
						}
					}

					if (info.Border > 0)
					{
						for (int bx = -info.Border; bx <= info.Border; bx++)
						{
							for (int by = -info.Border; by <= info.Border; by++)
							{
								if (bx == 0 && by == 0)
									continue;

								var borderPos = new MPos(x + by, y + bx);

								if (borderPos.X < 0 || borderPos.Y < 0)
									continue;
								if (borderPos.X >= Bounds.X || borderPos.Y >= Bounds.Y)
									continue;

								if (UsedCells[borderPos.X, borderPos.Y])
									continue;

								if (Loader.CanAcquireCell(borderPos, info.ID))
									Loader.SetTerrain(borderPos.X, borderPos.Y, info.BorderTerrain[Random.Next(info.BorderTerrain.Length)]);
							}
						}
					}
				}
			}

			MapPrinter.PrintGeneratorMap(Bounds, noise, UsedCells, info.ID);
		}
	}
}