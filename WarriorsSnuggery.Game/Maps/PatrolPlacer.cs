using System;
using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Objects.Actors;
using WarriorsSnuggery.Objects.Actors.Bot;

namespace WarriorsSnuggery.Maps
{
	[Desc("Used for spawning actor patrols on the map.")]
	public class PatrolPlacerInfo
	{
		[Desc("Finetuning of determining a valid spawnlocation.", "The lower, the more spawn locations will be found.")]
		public readonly int ScanSize = 3;
		[Desc("Minimum number of patrols per 32x32 field.")]
		public readonly int MinimumPatrols = 1;
		[Desc("Maximum number of patrols per 32x32 field.")]
		public readonly int MaximumPatrols = 4;
		[Desc("Team that these patrols belong to.")]
		public readonly byte Team = 1;
		[Desc("Decides whether these patrols are bots.")]
		public readonly bool AreBots = true;
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
		[Desc("Targeted distance between the spawned objects.", "Note that this will not be guaranteed in almost all cases.")]
		public readonly int ObjectMargin = 256;
		[Desc("What the patrol consists of.")]
		public readonly ActorType[] ActorTypes = new ActorType[0];
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

		readonly PatrolPlacerInfo info;
		readonly bool[,] invalidTerrain;

		public PatrolPlacer(Random random, World world, PatrolPlacerInfo info, in bool[,] invalidTerrain = default)
		{
			this.random = random;
			this.world = world;
			this.info = info;
			this.invalidTerrain = invalidTerrain;
		}

		public List<Actor> PlacePatrols()
		{
			var positions = new List<MPos>();
			var map = world.Map;

			positions.AddRange(map.PatrolSpawnLocations);
			// Clean up the available positions
			positions.RemoveAll(p => p.InRange(map.PlayableOffset, map.PlayableBounds + map.PlayableOffset));

			for (int x = info.ScanSize; x < map.PlayableBounds.X - info.ScanSize; x += info.ScanSize)
			{
				for (int y = info.ScanSize; y < map.PlayableBounds.Y - info.ScanSize; y += info.ScanSize)
				{
					var pos = map.PlayableOffset + new MPos(x, y);
					if (positions.Contains(pos) || (invalidTerrain != null && invalidTerrain[x, y]))
						continue;

					positions.Add(pos);
				}
			}

			var multiplier = map.PlayableBounds.X * map.PlayableBounds.Y / (float)(32 * 32) + (world.Game.Save.Difficulty - 5) / 10f;
			var count = random.Next((int)(info.MinimumPatrols * multiplier), (int)(info.MaximumPatrols * multiplier));
			if (positions.Count < count)
			{
				Log.Warning($"Unable to spawn patrol count ({count}) because there are not enough available spawn points ({positions.Count}).");
				count = positions.Count;
			}

			MPos selectRandomSpawn()
			{
				var index = random.Next(positions.Count);
				var spawn = positions[index];
				positions.RemoveAt(index);

				return spawn;
			}

			var actors = new List<Actor>();

			var maxAttempts = 10 * count;
			var failedAttempts = 0;
			while (count-- > 0)
			{
				var mid = selectRandomSpawn();
				var patrol = getPatrol();
				var unitCount = patrol.ActorTypes.Length;

				var types = new List<ActorType>(patrol.ActorTypes);
				var group = ActorDistribution.DistributeAround(world, mid.ToCPos(), types, patrol.ObjectMargin, info.Team, info.AreBots);

				if (group.Count == 0)
				{
					count++;
					failedAttempts++;

					if (failedAttempts > maxAttempts)
					{
						Log.Warning($"Unable to spawn remaining patrols ({count}) because the map is too crowded.");
						break;
					}

					continue;
				}

				actors.AddRange(group);

				var groupPatrol = new Patrol(group);
				foreach (var actor in group)
				{
					if (actor.Bot != null)
						actor.Bot.Patrol = groupPatrol;
				}

				map.PatrolSpawnedLocations.Add(mid);
			}

			return actors;
		}

		PatrolProbabilityInfo getPatrol()
		{
			var probability = (float)(random.NextDouble() * info.PatrolProbabilities);
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
