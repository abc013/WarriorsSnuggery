using System;
using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Maps.Noises;

namespace WarriorsSnuggery.Maps.Generators
{

	[Desc("Generator used for making paths.")]
	public sealed class PathGeneratorInfo : IMapGeneratorInfo
	{
		public int ID => id;
		readonly int id;

		[Desc("ID for the noisemap.", "Set to a negative value to not use one.")]
		public readonly int NoiseMapID = -1;

		[Desc("Width of the path.")]
		public readonly int Width = 2;

		[Desc("Terrain to use as roadtiles.")]
		public readonly ushort[] Types = new ushort[] { 0 };

		[Desc("Maximum count of paths.")]
		public readonly int MaxCount = 5;
		[Desc("Minimum count of paths.")]
		public readonly int MinCount = 1;

		[Desc("Defines to what percentage the road should be overgrown.")]
		public readonly float Ruinous = 0.1f;
		[Desc("Defines to what percentage the road should be overgrown.", "The first value will be used in the mid, the last at the edges of the map. Those in between will be used on linear scale.")]
		public readonly float[] RuinousFalloff = new[] { 0f, 0f, 0.1f, 0.2f, 0.3f, 0.8f };

		[Desc("If true, the road will not go as straight line but a curvy one.")]
		public readonly bool Curvy = true;

		public PathGeneratorInfo(int id, List<TextNode> nodes)
		{
			this.id = id;
			TypeLoader.SetValues(this, nodes);
		}

		public MapGenerator GetGenerator(Random random, MapLoader loader)
		{
			return new PathGenerator(random, loader, this);
		}
	}

	public class PathGenerator : MapGenerator
	{
		readonly PathGeneratorInfo info;

		readonly NoiseMap noise;
		readonly List<MPos> points = new List<MPos>();

		public PathGenerator(Random random, MapLoader loader, PathGeneratorInfo info) : base(random, loader)
		{
			this.info = info;

			noise = Loader.GetNoise(info.NoiseMapID);
		}

		public override void Generate()
		{
			var count = Random.Next(info.MinCount, info.MaxCount);

			// To have maximum diversity, we should get each path two nodes plus two extra.
			var nodesToGenerate = (count * 2 + 2) - Loader.Waypoints.Count;
			for (int i = 0; i < nodesToGenerate; i++)
				Loader.Waypoints.Add(MapUtils.RandomPositionFromEdge(Random, 1, Bounds));

			var waypointEnds = Loader.Waypoints.Where(w => w.Type == WaypointType.END).ToList();
			var waypointPassages = Loader.Waypoints.Where(w => w.Type == WaypointType.PASSAGE).ToList();
			for (int i = 0; i < count; i++)
			{
				var previousIndex = Random.Next(waypointEnds.Count);
				var previous = waypointEnds[previousIndex];
				waypointEnds.RemoveAt(previousIndex);

				var passageCount = Math.Clamp(Random.Next(3), 0, waypointPassages.Count);

				for (int j = 0; j < passageCount; j++)
				{
					var current = waypointPassages[Random.Next(waypointPassages.Count)];

					generateSingle(previous.Position, current.Position);

					previous = current;
				}

				var nextIndex = Random.Next(waypointEnds.Count);
				var next = waypointEnds[nextIndex];
				waypointEnds.RemoveAt(nextIndex);

				generateSingle(previous.Position, next.Position);
			}

			markDirty();
			drawDirty();

			MapPrinter.PrintGeneratorMap(Bounds, noise, UsedCells, info.ID);
		}

		public void Generate(bool[,] dirt)
		{
			UsedCells = dirt;
			drawDirty();

			MapPrinter.PrintGeneratorMap(Bounds, noise, UsedCells, info.ID);
		}

		void generateSingle(MPos start, MPos end)
		{
			var currentPosition = start;
			while (currentPosition != end)
			{
				var angle = end.AngleTo(currentPosition);
				var optX = MathF.Cos(angle);
				var x = (Math.Abs(optX) > 0.25f ? 1 : 0) * Math.Sign(optX);

				var optY = MathF.Sin(angle);
				var y = (Math.Abs(optY) > 0.25f ? 1 : 0) * Math.Sign(optY);

				// Maximal abeviation of +-45°
				if (info.Curvy)
				{
					MPos a1 = MPos.Zero;
					MPos a2 = MPos.Zero;
					// Get the two other possibilities near the best tone
					if (x != 0 && y != 0)
					{
						a1 = new MPos(x, 0) + currentPosition;
						a2 = new MPos(0, y) + currentPosition;
					}
					if (x == 0)
					{
						a1 = new MPos(1, y) + currentPosition;
						a2 = new MPos(-1, y) + currentPosition;
					}
					if (y == 0)
					{
						a1 = new MPos(x, 1) + currentPosition;
						a2 = new MPos(x, -1) + currentPosition;
					}
					MPos preferred = new MPos(x, y) + currentPosition;

					// If out of bounds, make the value high af so the field can't be taken
					var val1 = preferred.InRange(MPos.Zero, Bounds) ? noise[preferred.X, preferred.Y] : 10f;
					var val2 = a1.InRange(MPos.Zero, Bounds) ? noise[a1.X, a1.Y] : 10f;
					var val3 = a2.InRange(MPos.Zero, Bounds) ? noise[a2.X, a2.Y] : 10f;

					if (preferred != end)
					{
						if (val2 < val3 && val2 < val1 || a1 == end)
							preferred = a1;
						else if (val3 < val2 && val3 < val1 || a2 == end)
							preferred = a2;
					}

					currentPosition = preferred;
				}
				else
					currentPosition += new MPos(x, y);

				points.Add(currentPosition);
			}
		}

		void markDirty()
		{
			var width = (int)Math.Floor(info.Width / 2f);
			var width2 = info.Width - width;
			foreach (var point in points)
			{
				for (int x = point.X - width; x < point.X + width2; x++)
				{
					if (x >= 0 && x < Bounds.X)
					{
						for (int y = point.Y - width; y < point.Y + width2; y++)
						{
							if (y >= 0 && y < Bounds.Y)
							{
								if (Loader.AcquireCell(new MPos(x, y), info.ID, denyPatrols: false))
									UsedCells[x, y] = true;
							}
						}
					}
				}
			}
		}

		void drawDirty()
		{
			float distBetween = Center.Dist / info.RuinousFalloff.Length;
			for (int x = 0; x < Bounds.X; x++)
			{
				for (int y = 0; y < Bounds.Y; y++)
				{
					if (!UsedCells[x, y])
						continue;

					var ruinous = info.Ruinous;
					var ruinousLength = info.RuinousFalloff.Length;
					if (ruinousLength > 1)
					{
						var dist = (new MPos(x, y) - Center).Dist;

						var low = (int)Math.Floor(dist / distBetween);
						if (low >= ruinousLength)
							low = ruinousLength - 1;

						var high = (int)Math.Ceiling(dist / distBetween);
						if (high >= ruinousLength)
							high = ruinousLength - 1;

						var percent = (dist - low) / dist;

						ruinous += info.RuinousFalloff[low] * (1 - percent) + info.RuinousFalloff[high] * percent;
					}
					else
						ruinous += info.RuinousFalloff[0];

					if (Random.NextDouble() > ruinous)
					{
						var ran = Random.Next(info.Types.Length);
						Loader.SetTerrain(x, y, info.Types[ran]);

						var pos = new MPos(x, y);

						Loader.PatrolSpawnLocations.Add(pos);
					}
				}
			}
		}
	}
}
