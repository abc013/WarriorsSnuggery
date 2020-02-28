using System;
using System.Collections.Generic;
using System.Linq;

namespace WarriorsSnuggery.Maps
{
	public class PatrolGenerator : MapGenerator
	{
		readonly PatrolGeneratorInfo info;

		readonly List<MPos> positions = new List<MPos>();

		MPos[] spawns;

		public PatrolGenerator(Random random, Map map, World world, PatrolGeneratorInfo info) : base(random, map, world)
		{
			this.info = info;
		}

		public override void Generate()
		{
			MarkDirty();
			DrawDirty();
			ClearDirty();
		}

		protected override void MarkDirty()
		{
			for (int a = 0; a < Math.Floor(map.Bounds.X / (float)info.SpawnBounds); a++)
			{
				for (int b = 0; b < Math.Floor(map.Bounds.X / (float)info.SpawnBounds); b++)
				{
					var blocked = false;
					for (int x = a * info.SpawnBounds; x < a * info.SpawnBounds + info.SpawnBounds; x++)
					{
						if (x < map.TopLeftCorner.X || x >= map.TopRightCorner.X)
							continue;

						for (int y = a * info.SpawnBounds; y < a * info.SpawnBounds + info.SpawnBounds; y++)
						{
							if (y < map.TopLeftCorner.Y || y >= map.BottomLeftCorner.Y)
								continue;

							if (!info.UseForWaves && !map.CanAcquireCell(new MPos(x, y), info.ID))
								blocked = true;
						}
					}

					if (!blocked)
						positions.Add(new MPos(a * info.SpawnBounds, b * info.SpawnBounds));
				}
			}

			var multiplier = map.Bounds.X * map.Bounds.Y / (float) (32 * 32);
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
		}

		protected override void DrawDirty()
		{
			foreach (var spawn in spawns)
			{
				var mid = spawn.ToCPos();
				var patrol = getPatrol();
				var unitCount = patrol.ActorTypes.Length;

				for (int j = 0; j < unitCount; j++)
				{
					var spawnPosition = CPos.Zero;
					if (j == 0)
					{
						spawnPosition = mid;
					}
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

					if (spawnPosition.X < patrol.DistanceBetweenObjects / 2)
						spawnPosition = new CPos(patrol.DistanceBetweenObjects / 2, spawnPosition.Y, 0);
					if (spawnPosition.X >= map.Bounds.X * 1024 - patrol.DistanceBetweenObjects / 2)
						spawnPosition = new CPos(map.Bounds.X * 1024 - patrol.DistanceBetweenObjects / 2, spawnPosition.Y, 0);

					if (spawnPosition.Y < patrol.DistanceBetweenObjects / 2)
						spawnPosition = new CPos(spawnPosition.X, patrol.DistanceBetweenObjects / 2, 0);
					if (spawnPosition.Y >= map.Bounds.Y * 1024 - patrol.DistanceBetweenObjects / 2)
						spawnPosition = new CPos(spawnPosition.X, map.Bounds.Y * 1024 - patrol.DistanceBetweenObjects / 2, 0);

					world.Add(ActorCreator.Create(world, patrol.ActorTypes[j], spawnPosition, 1, true));
				}
			}
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

			return info.Patrols.FirstOrDefault();
		}

		protected override void ClearDirty()
		{
			positions.Clear();
			spawns = null;
		}
	}

	[Desc("Generator used for spawning enemies on the map.")]
	public class PatrolGeneratorInfo : MapGeneratorInfo
	{
		[Desc("Unique ID for the generator.")]
		public readonly new int ID;

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

		public PatrolGeneratorInfo(int id, MiniTextNode[] nodes) : base(id)
		{
			Loader.PartLoader.SetValues(this, nodes);

			if (id >= 0)
				PatrolProbabilities = Patrols.Sum(p => p.Probability);
		}

		public override MapGenerator GetGenerator(Random random, Map map, World world)
		{
			if (world.Game.Mode == GameMode.WAVES)
				return null;

			return new PatrolGenerator(random, map, world, this);
		}
	}

	[Desc("Information used for determining what actors will be spawned in the PatrolGenerator.")]
	public class PatrolProbabilityInfo
	{
		[Desc("Distance between the spawned objects in CPos size.", "Should be smaller than half of the size of the SpawnBounds.")]
		public readonly int DistanceBetweenObjects = 1024;
		[Desc("What the patrol consists of.")]
		public readonly string[] ActorTypes = new string[0];
		[Desc("Probability that this patrol will be spawned.", "This value will be set in relation with all other patrol probabilities.")]
		public readonly float Probability = 1f;

		public PatrolProbabilityInfo(MiniTextNode[] nodes)
		{
			Loader.PartLoader.SetValues(this, nodes);
		}
	}
}
