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

		public readonly Dictionary<int, NoiseMap> NoiseMaps = new Dictionary<int, NoiseMap>();
		public readonly List<Waypoint> Waypoints = new List<Waypoint>();
		public readonly List<MPos> PatrolSpawnLocations = new List<MPos>();

		public Map(World world, MapType type, int seed, int level, int difficulty)
		{
			this.world = world;

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
			CameraVisibility.SetBounds(this, world.ShroudLayer);

			var mapLoader = new MapLoader(world, this);
			mapLoader.Generate();
			mapLoader.Apply();
		}
	}
}
