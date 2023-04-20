using System;

namespace WarriorsSnuggery.Maps.Generators
{
	public interface IMapGeneratorInfo
	{
		int ID { get; }

		public abstract MapGenerator GetGenerator(Random random, MapLoader loader);
	}

	public abstract class MapGenerator
	{
		protected MPos PlayableOffset => Loader.PlayableOffset;
		protected MPos PlayableBounds => Loader.PlayableBounds;
		protected MPos Bounds => Loader.Bounds;
		protected MPos Center => Loader.Center;

		protected readonly Random Random;
		protected readonly MapLoader Loader;

		protected bool[,] UsedCells;

		protected MapGenerator(Random random, MapLoader loader)
		{
			Random = random;
			Loader = loader;

			UsedCells = new bool[Bounds.X, Bounds.Y];
		}

		public abstract void Generate();
	}
}
