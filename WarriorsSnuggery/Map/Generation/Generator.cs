using System;

namespace WarriorsSnuggery.Maps
{
	public abstract class MapGenerator
	{
		protected readonly Random random;

		protected readonly Map map;
		protected readonly World world;

		protected bool[,] dirtyCells;

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
		protected abstract void ClearDirty();
	}
}
