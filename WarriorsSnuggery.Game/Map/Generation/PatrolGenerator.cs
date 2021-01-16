﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace WarriorsSnuggery.Maps.Generators
{
	[Desc("Generator used for spawning enemies on the map.")]
	public class PatrolGeneratorInfo : IMapGeneratorInfo
	{
		public int ID => id;
		readonly int id;

		[Desc("Bounds of the patrol group to determine a valid spawnlocation.")]
		public readonly int SpawnBounds = 3;
		[Desc("Minimum number of patrols per 32x32 field.")]
		public readonly int MinimumPatrols = 1;
		[Desc("Maximum number of patrols per 32x32 field.")]
		public readonly int MaximumPatrols = 4;
		[Desc("Patrols to possibly spawn.")]
		public readonly PatrolProbabilityInfo[] Patrols;
		public readonly float PatrolProbabilities;

		[Desc("Use Patrols in the WAVES GameMode.", "Please note by setting to true, the Generator will not be used in other GameModes.")]
		public readonly bool UseForWaves;

		public PatrolGeneratorInfo(int id, List<MiniTextNode> nodes)
		{
			this.id = id;
			Loader.PartLoader.SetValues(this, nodes);

			if (Patrols != null && Patrols.Length != 0)
				PatrolProbabilities = Patrols.Sum(p => p.Probability);
		}

		public MapGenerator GetGenerator(Random random, MapLoader loader)
		{
			// TODO move elsewhere
			if (loader.ObjectiveType == ObjectiveType.SURVIVE_WAVES)
				return null;

			return new PatrolGenerator(random, loader, this);
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

		public PatrolProbabilityInfo(List<MiniTextNode> nodes)
		{
			Loader.PartLoader.SetValues(this, nodes);
		}
	}

	public class PatrolGenerator : MapGenerator
	{
		readonly PatrolGeneratorInfo info;

		readonly List<MPos> positions = new List<MPos>();

		MPos[] spawns;

		public PatrolGenerator(Random random, MapLoader loader, PatrolGeneratorInfo info) : base(random, loader)
		{
			this.info = info;
		}

		public override void Generate()
		{
			for (int a = 0; a < Math.Floor(Bounds.X / (float)info.SpawnBounds); a++)
			{
				for (int b = 0; b < Math.Floor(Bounds.X / (float)info.SpawnBounds); b++)
				{
					if (!areaBlocked(a, b))
						positions.Add(new MPos(a * info.SpawnBounds, b * info.SpawnBounds));
				}
			}

			var multiplier = Bounds.X * Bounds.Y / (float)(32 * 32) + (Loader.Statistics.Difficulty - 5) / 10f;
			var count = Random.Next((int)(info.MinimumPatrols * multiplier), (int)(info.MaximumPatrols * multiplier));
			if (positions.Count < count)
			{
				Log.WriteDebug(string.Format("Unable to spawn Patrol count ({0}) because there are not enough available spawn points ({1}).", count, positions.Count));
				count = positions.Count;
			}

			spawns = new MPos[count];

			for (int i = 0; i < count; i++)
			{
				var posIndex = Random.Next(positions.Count);
				spawns[i] = positions[posIndex];
				positions.RemoveAt(posIndex);
			}

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

					if (spawnPosition.X < TopLeftCorner.X + patrol.DistanceBetweenObjects / 2)
						spawnPosition = new CPos(TopLeftCorner.X + patrol.DistanceBetweenObjects / 2, spawnPosition.Y, 0);
					else if (spawnPosition.X >= BottomRightCorner.X - patrol.DistanceBetweenObjects / 2)
						spawnPosition = new CPos(BottomRightCorner.X - patrol.DistanceBetweenObjects / 2, spawnPosition.Y, 0);

					if (spawnPosition.Y < TopLeftCorner.Y + patrol.DistanceBetweenObjects / 2)
						spawnPosition = new CPos(spawnPosition.X, TopLeftCorner.Y + patrol.DistanceBetweenObjects / 2, 0);
					else if (spawnPosition.Y >= BottomRightCorner.Y - patrol.DistanceBetweenObjects / 2)
						spawnPosition = new CPos(spawnPosition.X, BottomRightCorner.Y - patrol.DistanceBetweenObjects / 2, 0);

					Loader.AddActor(spawnPosition, patrol.ActorTypes[j], patrol.Team, true);
				}
			}
		}

		bool areaBlocked(int a, int b)
		{
			for (int x = a * info.SpawnBounds; x < a * info.SpawnBounds + info.SpawnBounds; x++)
			{
				if (x < TopLeftCorner.X || x >= TopRightCorner.X)
					continue;

				for (int y = b * info.SpawnBounds; y < b * info.SpawnBounds + info.SpawnBounds; y++)
				{
					if (y < TopLeftCorner.Y || y >= BottomLeftCorner.Y)
						continue;

					if (!Loader.CanAcquireCell(new MPos(x, y), info.ID))
						return true;
				}
			}

			return false;
		}

		PatrolProbabilityInfo getPatrol()
		{
			var probability = Random.NextDouble() * info.PatrolProbabilities;
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