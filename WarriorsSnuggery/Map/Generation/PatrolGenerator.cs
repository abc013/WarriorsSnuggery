using System;
using System.Collections.Generic;

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
						if (x < 0 || x >= map.Bounds.X)
							continue;

						for (int y = a * info.SpawnBounds; y < a * info.SpawnBounds + info.SpawnBounds; y++)
						{
							if (y < 0 || y >= map.Bounds.Y)
								continue;

							if (!map.CanAcquireCell(new MPos(x, y), info.ID))
								blocked = true;
						}
					}

					if (!blocked)
						positions.Add(new MPos(a * info.SpawnBounds, b * info.SpawnBounds));
				}
			}

			var count = random.Next(info.MinimumPatrols, info.MaximumPatrols);
			if (positions.Count < count)
				count = positions.Count;
			
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
				var mid = spawn.ToCPos() + new CPos(spawn.X * 512, spawn.Y * 512, 0);
				var patrol = info.Patrols[random.Next(info.Patrols.Length)];
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
						var deltaX = (int)(info.DistanceBetweenObjects * Math.Sin(angle));
						var deltaY = (int)(info.DistanceBetweenObjects * Math.Cos(angle));

						spawnPosition = mid + new CPos(deltaX, deltaY, 0);
					}
					else if (j < 19)
					{
						var angle = 30 * (j - 6) / 180f * Math.PI;
						var deltaX = (int)(info.DistanceBetweenObjects * 2 * Math.Sin(angle));
						var deltaY = (int)(info.DistanceBetweenObjects * 2 * Math.Cos(angle));

						spawnPosition = mid + new CPos(deltaX, deltaY, 0);
					}

					if (spawnPosition.X < info.DistanceBetweenObjects / 2 || spawnPosition.X >= map.Bounds.X * 1024 - info.DistanceBetweenObjects / 2)
						continue;
					if (spawnPosition.Y < info.DistanceBetweenObjects / 2 || spawnPosition.Y >= map.Bounds.Y * 1024 - info.DistanceBetweenObjects / 2)
						continue;

					world.Add(ActorCreator.Create(world, patrol.ActorTypes[j], spawnPosition, 1, true));
				}
			}
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

		[Desc("Minimum number of patrols.")]
		public readonly int MinimumPatrols = 1;
		[Desc("Maximum number of patrols.", "Maximum currently is 13.")]
		public readonly int MaximumPatrols = 4;
		[Desc("Distance between the spawned objects in CPos size.", "Should be smaller than half of the size of the SpawnBounds.")]
		public readonly int DistanceBetweenObjects = 1024;
		[Desc("Distance between the spawned objects in Cell size.")]
		public readonly int SpawnBounds = 3;
		[Desc("Patrols to possibly spawn.")]
		public readonly PatrolProbabilityGeneratorInfo[] Patrols;

		public PatrolGeneratorInfo(int id, MiniTextNode[] nodes) : base(id)
		{
			Loader.PartLoader.SetValues(this, nodes);
		}

		public override MapGenerator GetGenerator(Random random, Map map, World world)
		{
			return new PatrolGenerator(random, map, world, this);
		}
	}

	[Desc("Information used for determining what actors will be spawned in the PatrolGenerator.")]
	public class PatrolProbabilityGeneratorInfo
	{
		[Desc("What the patrol consists of.")]
		public readonly string[] ActorTypes = new string[0];

		public PatrolProbabilityGeneratorInfo(MiniTextNode[] nodes)
		{
			Loader.PartLoader.SetValues(this, nodes);
		}
	}
}
