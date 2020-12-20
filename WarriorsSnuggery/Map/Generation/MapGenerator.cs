using System;

namespace WarriorsSnuggery.Maps.Generators
{
	public abstract class MapGeneratorInfo
	{
		public readonly int ID;

		public MapGeneratorInfo(int id)
		{
			ID = id;
		}

		public abstract MapGenerator GetGenerator(Random random, MapLoader loader);
	}

	public abstract class MapGenerator
	{
		protected readonly Random random;

		protected readonly MapLoader loader;

		protected MPos Bounds => loader.Bounds;
		public MPos Center => loader.Center;
		public CPos TopLeftCorner => loader.TopLeftCorner;
		public CPos TopRightCorner => loader.TopRightCorner;
		public CPos BottomLeftCorner => loader.BottomLeftCorner;
		public CPos BottomRightCorner => loader.BottomRightCorner;
		public CPos PlayerSpawn => loader.PlayerSpawn;
		public CPos Exit => loader.Exit;

		protected bool[,] dirtyCells;

		protected MapGenerator(Random random, MapLoader loader)
		{
			this.random = random;
			this.loader = loader;

			dirtyCells = new bool[Bounds.X, Bounds.Y];
		}

		public abstract void Generate();
	}
}
