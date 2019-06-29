using System;
using System.Collections.Generic;

namespace WarriorsSnuggery.Maps
{
	public abstract class MapGenerator
	{
		protected readonly Random random;

		protected readonly Map map;
		protected readonly World world;

		protected readonly bool[,] dirtyCells;

		protected MapGenerator(Random random, Map map, World world)
		{
			this.random = random;
			this.map = map;
			this.world = world;
			dirtyCells = new bool[map.Bounds.X, map.Bounds.Y];
		}

		public abstract void Generate();

		protected abstract void MarkDirty();
		protected abstract void DrawDirty();
	}

	public class PathGenerator : MapGenerator
	{
		readonly PathGenerationType type;

		readonly float[,] noise;

		readonly List<MPos> points = new List<MPos>();

		public PathGenerator(Random random, Map map, World world, PathGenerationType type) : base(random, map, world)
		{
			this.type = type;

			noise = new float[map.Bounds.X, map.Bounds.Y];
		}

		public override void Generate()
		{
			var rawnoise = Noise.GenerateClouds(map.Bounds, random);
			for (int y = 0; y < map.Bounds.Y; y++)
			{
				for (int x = 0; x < map.Bounds.X; x++)
				{
					noise[x, y] = rawnoise[y * map.Bounds.X + x];
				}
			}
			var count = random.Next(type.MaxCount - type.MinCount) + type.MinCount;
			for (int i = 0; i < count; i++)
			{
				MPos start = type.FromEntrance ? map.PlayerSpawn.ToMPos() : MapUtils.RandomPositionInMap(random, 1, map.Bounds);
				MPos end = type.ToExit ? map.Exit : MapUtils.RandomPositionFromEdge(random, 1, map.Bounds);

				generateSingle(start, end);
			}

			MarkDirty();
			DrawDirty();
		}

		void generateSingle(MPos start, MPos end)
		{
			var currentPosition = start;
			while(currentPosition != end)
			{
				var angle = end.AngleTo(currentPosition);
				var optX = Math.Cos(angle);
				var x = (Math.Abs(optX) > 0.25f ? 1 : 0) * Math.Sign(optX);

				var optY = Math.Sin(angle);
				var y = (Math.Abs(optY) > 0.25f ? 1 : 0) * Math.Sign(optY);

				// Maximal abeviation of +-45°
				if (type.Curvy)
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
					var val1 = preferred.IsInRange(MPos.Zero, map.Bounds - new MPos(1,1)) ? noise[preferred.X, preferred.Y] : 10f;
					var val2 = a1.IsInRange(MPos.Zero, map.Bounds - new MPos(1, 1)) ? noise[a1.X, a1.Y] : 10f;
					var val3 = a2.IsInRange(MPos.Zero, map.Bounds - new MPos(1, 1)) ? noise[a2.X, a2.Y] : 10f;

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
				{
					currentPosition += new MPos(x, y);
				}
				points.Add(currentPosition);
			}
		}

		protected override void MarkDirty()
		{
			foreach(var point in points)
			{
				for (int x = point.X - type.Size; x < point.X + type.Size; x++)
				{
					if (x >= 0 && x < map.Bounds.X)
					{
						for (int y = point.Y - type.Size; y < point.Y + type.Size; y++)
						{
							if (y >= 0 && y < map.Bounds.Y)
							{
								dirtyCells[x, y] = true;
							}
						}
					}
				}
			}
		}

		protected override void DrawDirty()
		{
			float distBetween = MPos.Zero.DistTo(map.Mid) / type.RuinousFalloff.Length;
			for (int x = 0; x < map.Bounds.X; x++)
			{
				for (int y = 0; y < map.Bounds.Y; y++)
				{
					if (!dirtyCells[x, y])
						continue;

					var ruinous = type.Ruinous;
					var ruinousLength = type.RuinousFalloff.Length;
					if (ruinousLength > 1)
					{
						var dist = new MPos(x, y).DistTo(map.Mid);

						var low = (int) Math.Floor(dist / distBetween);
						var high = (int)Math.Ceiling(dist / distBetween);
						if (high >= ruinousLength)
							high = ruinousLength - 1;

						var percent = (dist - low) / dist;

						ruinous += type.RuinousFalloff[low] * (1 - percent) + type.RuinousFalloff[high] * percent;
					}
					else
					{
						ruinous += type.RuinousFalloff[0];
					}

					if (random.NextDouble() > ruinous)
					{
						var ran = random.Next(type.Types.Length);
						world.TerrainLayer.Set(TerrainCreator.Create(world, new WPos(x, y, 0), type.Types[ran]));
					}
				}
			}
		}
	}
}
