using System;
using System.Collections.Generic;
using System.Linq;

namespace WarriorsSnuggery.Maps
{
	public static class MapUtils
	{
		public static readonly MPos MinimumMapBounds = new MPos(64, 64);
		public static readonly MPos MaximumMapBounds = new MPos(512, 512);

		public static MPos[] FindValuesInArea(MPos position, int searchRadius, int[] value, int[,] array, MPos bounds)
		{
			var positions = new List<MPos>();
			for (var x = position.X - searchRadius; x < position.X + searchRadius; x++)
			{
				if (x < 0 || x >= bounds.X)
					continue;

				for (var y = position.Y - searchRadius; y < position.Y + searchRadius; y++)
				{
					if (y < 0 || y >= bounds.Y)
						continue;

					if (value.Contains(array[x, y]))
						positions.Add(new MPos(x, y));
				}
			}

			return positions.ToArray();
		}

		public static Waypoint RandomPositionInMap(Random random, int distanceToMapEdge, MPos bounds)
		{
			var xSize = bounds.X - distanceToMapEdge * 2;
			if (xSize < 0)
				xSize = 0;

			var ySize = bounds.X - distanceToMapEdge * 2;
			if (ySize < 0)
				ySize = 0;

			var x = distanceToMapEdge + random.Next(xSize);
			var y = distanceToMapEdge + random.Next(ySize);

			return new Waypoint(new MPos(x, y), WaypointLocation.CENTER, WaypointType.PASSAGE);
		}

		public static Waypoint RandomPositionFromEdge(Random random, int distanceFromMapEdge, MPos bounds)
		{
			MPos pos = MPos.Zero;
			WaypointLocation location = WaypointLocation.CENTER;

			var side = (byte)random.Next(4);
			switch (side)
			{
				case 0:
					pos = new MPos(distanceFromMapEdge, distanceFromMapEdge + random.Next(bounds.Y - distanceFromMapEdge * 2));
					location = WaypointLocation.TOP;
					break;
				case 1:
					pos = new MPos(distanceFromMapEdge + random.Next(bounds.X - distanceFromMapEdge * 2), distanceFromMapEdge);
					location = WaypointLocation.LEFT;
					break;
				case 2:
					pos = new MPos(bounds.X - distanceFromMapEdge - 1, distanceFromMapEdge + random.Next(bounds.Y - distanceFromMapEdge * 2));
					location = WaypointLocation.BOTTOM;
					break;
				case 3:
					pos = new MPos(distanceFromMapEdge + random.Next(bounds.X - distanceFromMapEdge * 2), bounds.Y - distanceFromMapEdge - 1);
					location = WaypointLocation.RIGHT;
					break;
			}

			return new Waypoint(pos, location, WaypointType.END);
		}

		public static MPos RandomMapBounds(Random random, int difficulty, int level)
		{
			var minimum = MinimumMapBounds;
			var maximum = MaximumMapBounds;

			// Multiplier, in about a range from 1 to 10
			var multipier = Math.Sqrt((difficulty + 1) * (level + 1));

			// Multiplier, in a range from 5 to 10
			var sizeMultiplier = 10;

			// Little variation from -5 to 5;
			var variation = random.Next(difficulty * 2 + 2) - difficulty - 1;

			var valueX = (int)(multipier * sizeMultiplier) + variation;
			var valueY = valueX;

			valueX = Math.Clamp(valueX, minimum.X, maximum.X);
			valueY = Math.Clamp(valueY, minimum.Y, maximum.Y);

			return new MPos(valueX, valueY);
		}
	}
}
