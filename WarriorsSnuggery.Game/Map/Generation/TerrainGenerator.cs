using System;
using System.Collections.Generic;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.Maps.Generators
{
	[Desc("Generator used for generating random flocks of terrain or actors on the map.")]
	public class TerrainGeneratorInfo : IMapGeneratorInfo
	{
		public int ID => id;
		readonly int id;

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

		public TerrainGeneratorInfo(int id, List<MiniTextNode> nodes)
		{
			this.id = id;
			Loader.PartLoader.SetValues(this, nodes);

			if (RangeSteps.Length != ProbabilitySteps.Length)
				throw new InvalidTextNodeException($"Range step length ({RangeSteps.Length}) does not match with given provabability values ({ProbabilitySteps.Length}).");
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

		public ActorProbabilityInfo(List<MiniTextNode> nodes)
		{
			Loader.PartLoader.SetValues(this, nodes);

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
						for (int by = 0; by < info.Border * 2 + 1; by++)
						{
							for (int bx = 0; bx < info.Border * 2 + 1; bx++)
							{
								var p = new MPos(x + by - info.Border, y + bx - info.Border);

								if (p.X < 0 || p.Y < 0)
									continue;
								if (p.X >= Bounds.X || p.Y >= Bounds.Y)
									continue;

								if (!UsedCells[p.X, p.Y] && Loader.AcquireCell(p, info.ID, denyPatrols: false))
									Loader.SetTerrain(x, y, info.BorderTerrain[Random.Next(info.BorderTerrain.Length)]);
							}
						}
					}
				}
			}

			MapPrinter.PrintGeneratorMap(Bounds, noise, UsedCells, info.ID);
		}
	}
}