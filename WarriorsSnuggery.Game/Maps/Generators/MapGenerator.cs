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
		protected CPos TopLeftCorner => Loader.TopLeftCorner;
		protected CPos TopRightCorner => Loader.TopRightCorner;
		protected CPos BottomLeftCorner => Loader.BottomLeftCorner;
		protected CPos BottomRightCorner => Loader.BottomRightCorner;
		protected CPos PlayerSpawn => Loader.PlayerSpawn;
		protected CPos Exit => Loader.Exit;

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
