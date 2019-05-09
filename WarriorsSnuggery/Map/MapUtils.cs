using System;
using System.Collections.Generic;
using System.Linq;

namespace WarriorsSnuggery.Maps
{
	public static class MapUtils
	{
		public static MPos[] FindValuesInArea(MPos position, int searchRadius, int[] value, int[,] array, MPos size)
		{
			var positions = new List<MPos>();
			for(var x = position.X - searchRadius; x < position.X + searchRadius; x++)
			{
				if (x < 0 || x >= size.X)
					continue;

				for (var y = position.Y - searchRadius; y < position.Y + searchRadius; y++)
				{
					if (y < 0 || y >= size.Y)
						continue;

					if (value.Contains(array[x, y]))
						positions.Add(new MPos(x, y));
				}
			}

			return positions.ToArray();
		}

		public static MPos RandomPositionInMap(Random random, int distanceToMapEdge, MPos size)
		{
			var x = distanceToMapEdge + random.Next(size.X - distanceToMapEdge);
			var y = distanceToMapEdge + random.Next(size.Y - distanceToMapEdge);

			return new MPos(x, y);
		}
	}
}
