using System;
using System.Collections.Generic;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Maps.Noises;

namespace WarriorsSnuggery.Maps
{
	public class Map
	{
		readonly World world;
		readonly Random random;

		public readonly MapType Type;
		public readonly int Seed;

		public static readonly CPos Offset = new CPos(-Constants.TileSize / 2, -Constants.TileSize / 2, 0);
		public readonly MPos PlayableOffset;

		public CPos PlayerSpawn;
		public CPos Exit;

		public readonly MPos Bounds;
		public readonly MPos PlayableBounds;
		public readonly MPos Center;
		public readonly MPos DefaultEdgeDistance;

		public readonly CPos TopLeftCorner;
		public readonly CPos TopRightCorner;
		public readonly CPos BottomLeftCorner;
		public readonly CPos BottomRightCorner;

		public readonly Dictionary<int, NoiseMap> NoiseMaps = new Dictionary<int, NoiseMap>();
		public readonly List<Waypoint> Waypoints = new List<Waypoint>();
		public readonly List<MPos> PatrolSpawnLocations = new List<MPos>();
		public readonly List<MPos> PatrolSpawnedLocations = new List<MPos>();

		public Map(World world, MapType type, int seed, int level, int difficulty)
		{
			this.world = world;

			Type = type;
			Seed = seed;
			random = new Random(seed);

			PlayableBounds = determineBounds(difficulty, level);
			PlayableOffset = new MPos(type.WorldBorder, type.WorldBorder);
			Bounds = PlayableBounds + PlayableOffset * 2;

			// Cache positions
			Center = Bounds / 2;
			DefaultEdgeDistance = Bounds / 8;

			TopLeftCorner = Offset;
			TopRightCorner = Offset + new CPos(Bounds.X * Constants.TileSize, 0, 0);
			BottomLeftCorner = Offset + new CPos(0, Bounds.Y * Constants.TileSize, 0);
			BottomRightCorner = Offset + new CPos(Bounds.X * Constants.TileSize, Bounds.Y * Constants.TileSize, 0);

			PlayerSpawn = type.SpawnPoint;
			if (PlayerSpawn == CPos.Zero)
				PlayerSpawn = Center.ToCPos();
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

			var mapLoader = new MapLoader(world, this);
			mapLoader.Generate();
			mapLoader.Apply();
		}
	}
}
