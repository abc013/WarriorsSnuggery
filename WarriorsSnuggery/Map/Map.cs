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

		public readonly MapType Type;
		public readonly int Seed;

		public static readonly CPos Offset = new CPos(-512, -512, 0);

		public CPos PlayerSpawn;
		public CPos Exit;

		public readonly MPos Bounds;
		public readonly MPos Center;
		public readonly MPos DefaultEdgeDistance;

		public readonly CPos TopLeftCorner;
		public readonly CPos TopRightCorner;
		public readonly CPos BottomLeftCorner;
		public readonly CPos BottomRightCorner;

		public Dictionary<int, NoiseMap> NoiseMaps { get; private set; }
		public List<Waypoint> Waypoints { get; private set; }

		public Map(World world, MapType type, int seed, int level, int difficulty)
		{
			this.world = world;

			PlayerSpawn = type.SpawnPoint.ToCPos();
			Type = type;
			Seed = seed;
			random = new Random(seed);

			Bounds = determineBounds(difficulty, level);

			// Cache positions
			Center = Bounds / new MPos(2, 2);
			DefaultEdgeDistance = Bounds / new MPos(8, 8);

			TopLeftCorner = Offset;
			TopRightCorner = new CPos(Offset.X + (Bounds.X * 1024), Offset.Y, 0);
			BottomLeftCorner = new CPos(Offset.X, Offset.Y + (Bounds.Y * 1024), 0);
			BottomRightCorner = new CPos(Offset.X + (Bounds.X * 1024), Offset.Y + (Bounds.Y * 1024), 0);
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
			Waypoints = mapLoader.Waypoints;

			mapLoader.Generate();

			mapLoader.Apply();
		}
	}
}
