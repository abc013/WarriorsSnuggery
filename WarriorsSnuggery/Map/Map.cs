using System;
using System.Collections.Generic;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Maps;

namespace WarriorsSnuggery
{
	public enum NoiseType
	{
		NONE,
		NOISE,
		CLOUDS,
		MAZE
	}

	public class Map
	{
		readonly World world;
		readonly Random random;

		public readonly MapInfo Type;
		public readonly int Seed;

		public static readonly CPos Offset = new CPos(-512, -512, 0);

		public CPos PlayerSpawn;
		public CPos Exit;

		public MPos Bounds { get; private set; }
		public MPos Center => Bounds / new MPos(2, 2);
		public MPos DefaultEdgeDistance => Bounds / new MPos(8, 8);

		public CPos TopLeftCorner => Offset;
		public CPos TopRightCorner => new CPos(Offset.X + (Bounds.X * 1024), Offset.Y, 0);
		public CPos BottomLeftCorner => new CPos(Offset.X, Offset.Y + (Bounds.Y * 1024), 0);
		public CPos BottomRightCorner => new CPos(Offset.X + (Bounds.X * 1024), Offset.Y + (Bounds.Y * 1024), 0);

		public Dictionary<int, NoiseMap> NoiseMaps;

		public Map(World world, MapInfo type, int seed, int level, int difficulty)
		{
			this.world = world;

			PlayerSpawn = type.SpawnPoint.ToCPos();
			Type = type;
			Seed = seed;
			random = new Random(seed);

			Bounds = determineBounds(difficulty, level);
		}

		MPos determineBounds(int difficulty, int level)
		{
			if (Type.CustomSize != MPos.Zero)
				return Type.CustomSize;

			return MapUtils.RandomMapBounds(random, difficulty, level);
		}

		public void Load()
		{
			Camera.SetBounds(Bounds);
			VisibilitySolver.SetBounds(this, world.ShroudLayer);

			var mapLoader = new MapLoader(world, this);
			NoiseMaps = mapLoader.NoiseMaps;

			mapLoader.Generate();

			mapLoader.Apply();
		}
	}
}
