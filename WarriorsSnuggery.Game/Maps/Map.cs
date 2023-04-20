using System;
using System.Collections.Generic;

namespace WarriorsSnuggery.Maps
{
	public class Map
	{
		public readonly MapType Type;
		public readonly int Seed;
		public MapDebugInformation DebugInformation { get; internal set; }

		public static readonly CPos Offset = new CPos(-Constants.TileSize / 2, -Constants.TileSize / 2, 0);
		public readonly MPos PlayableOffset;

		public readonly MPos Bounds;
		public readonly MPos PlayableBounds;
		public readonly MPos Center;
		public readonly MPos DefaultEdgeDistance;

		public readonly CPos TopLeftCorner;
		public readonly CPos TopRightCorner;
		public readonly CPos BottomLeftCorner;
		public readonly CPos BottomRightCorner;

		public CPos PlayerSpawn { get; internal set; }
		public List<MPos> PossiblePatrolLocations { get; internal set; }

		public Map(Game game)
		{
			Type = game.MapType;
			Seed = game.Seed;

			PlayableOffset = new MPos(Type.WorldBorder, Type.WorldBorder);
			PlayableBounds = determineBounds(new Random(Seed), game.Save);
			Bounds = PlayableBounds + PlayableOffset * 2;

			// Cache positions
			Center = Bounds / 2;
			DefaultEdgeDistance = Bounds / 8;

			TopLeftCorner = Offset;
			TopRightCorner = Offset + new CPos(Bounds.X * Constants.TileSize, 0, 0);
			BottomLeftCorner = Offset + new CPos(0, Bounds.Y * Constants.TileSize, 0);
			BottomRightCorner = Offset + new CPos(Bounds.X * Constants.TileSize, Bounds.Y * Constants.TileSize, 0);

			PlayerSpawn = Type.SpawnPoint;
			if (PlayerSpawn == CPos.Zero)
				PlayerSpawn = Center.ToCPos();
		}

		MPos determineBounds(Random random, GameSave save)
		{
			if (Type.CustomSize != MPos.Zero)
				return Type.CustomSize - PlayableOffset * 2;

			return MapUtils.RandomMapBounds(random, save.Difficulty, save.Level);
		}
	}
}
