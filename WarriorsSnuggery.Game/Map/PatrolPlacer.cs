using System;
using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.Maps
{
	[Desc("Used for spawning actor patrols on the map.")]
	public class PatrolPlacerInfo
	{
		[Desc("Bounds of the patrol group to determine a valid spawnlocation.")]
		public readonly int SpawnBounds = 3;
		[Desc("Minimum number of patrols per 32x32 field.")]
		public readonly int MinimumPatrols = 1;
		[Desc("Maximum number of patrols per 32x32 field.")]
		public readonly int MaximumPatrols = 4;
		[Desc("Patrols to possibly spawn.")]
		public readonly PatrolProbabilityInfo[] Patrols = new PatrolProbabilityInfo[0];
		public readonly float PatrolProbabilities;

		[Desc("Only use this PatrolPlacer in the WAVES Gamemode.")]
		public readonly bool UseForWaves;

		public PatrolPlacerInfo(List<TextNode> nodes)
		{
			TypeLoader.SetValues(this, nodes);

			if (Patrols.Length != 0)
				PatrolProbabilities = Patrols.Sum(p => p.Probability);
		}
	}

	[Desc("Information used for determining what actors will be spawned in the PatrolGenerator.")]
	public class PatrolProbabilityInfo
	{
		[Desc("Distance between the spawned objects in CPos size.", "Should be smaller than half of the size of the SpawnBounds.")]
		public readonly int DistanceBetweenObjects = 1024;
		[Desc("What the patrol consists of.")]
		public readonly string[] ActorTypes = new string[0];
		[Desc("Team that the patrol belongs to.")]
		public readonly byte Team = 1;
		[Desc("Probability that this patrol will be spawned.", "This value will be set in relation with all other patrol probabilities.")]
		public readonly float Probability = 1f;

		public PatrolProbabilityInfo(List<TextNode> nodes)
		{
			TypeLoader.SetValues(this, nodes);
		}
	}

	public class PatrolPlacer
	{
		readonly Random random;

		readonly World world;
		readonly MPos bounds;

		readonly PatrolPlacerInfo info;

		bool[,] invalidTerrain;

		readonly List<MPos> positions = new List<MPos>();
		MPos[] spawns;

		public PatrolPlacer(Random random, World world, PatrolPlacerInfo info)
		{
			this.random = random;
			this.world = world;
			bounds = world.Map.Bounds;
			this.info = info;
		}

		public void SetInvalid(bool[,] invalidTerrain)
		{
			this.invalidTerrain = invalidTerrain;
		}

		public List<Actor> PlacePatrols()
		{
			var actors = new List<Actor>();

			for (int a = 0; a < Math.Floor(bounds.X / (float)info.SpawnBounds); a++)
			{
				for (int b = 0; b < Math.Floor(bounds.X / (float)info.SpawnBounds); b++)
				{
					if (!areaBlocked(a, b))
						positions.Add(new MPos(a * info.SpawnBounds, b * info.SpawnBounds));
				}
			}

			var multiplier = bounds.X * bounds.Y / (float)(32 * 32) + (world.Game.Statistics.Difficulty - 5) / 10f;
			var count = random.Next((int)(info.MinimumPatrols * multiplier), (int)(info.MaximumPatrols * multiplier));
			if (positions.Count < count)
			{
				Log.WriteDebug(string.Format("Unable to spawn Patrol count ({0}) because there are not enough available spawn points ({1}).", count, positions.Count));
				count = positions.Count;
			}

			spawns = new MPos[count];

			for (int i = 0; i < count; i++)
			{
				var posIndex = random.Next(positions.Count);
				spawns[i] = positions[posIndex];
				positions.RemoveAt(posIndex);
			}

			var map = world.Map;
			foreach (var spawn in spawns)
			{
				var mid = spawn.ToCPos();
				var patrol = getPatrol();
				var unitCount = patrol.ActorTypes.Length;

				for (int j = 0; j < unitCount; j++)
				{
					var spawnPosition = CPos.Zero;
					if (j == 0)
						spawnPosition = mid;
					else if (j < 7)
					{
						var angle = 60 * j / 180f * Math.PI;
						var deltaX = (int)(patrol.DistanceBetweenObjects * Math.Sin(angle));
						var deltaY = (int)(patrol.DistanceBetweenObjects * Math.Cos(angle));

						spawnPosition = mid + new CPos(deltaX, deltaY, 0);
					}
					else if (j < 19)
					{
						var angle = 30 * (j - 6) / 180f * Math.PI;
						var deltaX = (int)(patrol.DistanceBetweenObjects * 2 * Math.Sin(angle));
						var deltaY = (int)(patrol.DistanceBetweenObjects * 2 * Math.Cos(angle));

						spawnPosition = mid + new CPos(deltaX, deltaY, 0);
					}

					if (spawnPosition.X < map.TopLeftCorner.X + patrol.DistanceBetweenObjects / 2)
						spawnPosition = new CPos(map.TopLeftCorner.X + patrol.DistanceBetweenObjects / 2, spawnPosition.Y, 0);
					else if (spawnPosition.X >= map.BottomRightCorner.X - patrol.DistanceBetweenObjects / 2)
						spawnPosition = new CPos(map.BottomRightCorner.X - patrol.DistanceBetweenObjects / 2, spawnPosition.Y, 0);

					if (spawnPosition.Y < map.TopLeftCorner.Y + patrol.DistanceBetweenObjects / 2)
						spawnPosition = new CPos(spawnPosition.X, map.TopLeftCorner.Y + patrol.DistanceBetweenObjects / 2, 0);
					else if (spawnPosition.Y >= map.BottomRightCorner.Y - patrol.DistanceBetweenObjects / 2)
						spawnPosition = new CPos(spawnPosition.X, map.BottomRightCorner.Y - patrol.DistanceBetweenObjects / 2, 0);

					var actor = ActorCreator.Create(world, patrol.ActorTypes[j], spawnPosition, patrol.Team, true);

					world.Add(actor);
					actors.Add(actor);
				}
			}

			return actors;
		}

		bool areaBlocked(int a, int b)
		{
			var map = world.Map;
			for (int x = a * info.SpawnBounds; x < a * info.SpawnBounds + info.SpawnBounds; x++)
			{
				if (x < map.TopLeftCorner.X || x >= map.TopRightCorner.X)
					continue;

				for (int y = b * info.SpawnBounds; y < b * info.SpawnBounds + info.SpawnBounds; y++)
				{
					if (y < map.TopLeftCorner.Y || y >= map.BottomLeftCorner.Y)
						continue;

					if (invalidTerrain[x, y])
						return true;
				}
			}

			return false;
		}

		PatrolProbabilityInfo getPatrol()
		{
			var probability = random.NextDouble() * info.PatrolProbabilities;
			for (int i = 0; i < info.Patrols.Length; i++)
			{
				probability -= info.Patrols[i].Probability;

				if (probability < 0)
					return info.Patrols[i];
			}

			return info.Patrols.First();
		}
	}
}
