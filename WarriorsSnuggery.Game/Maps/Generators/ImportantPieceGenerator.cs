﻿using System;
using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Maps.Noises;
using WarriorsSnuggery.Maps.Pieces;
using WarriorsSnuggery.Objectives;

namespace WarriorsSnuggery.Maps.Generators
{
	public enum PositionType
	{
		POSITION,
		SPAWN,
		EXIT,
		KEY
	}

	[Desc("Generator used to generate pieces that must be on the map.")]
	public class ImportantPieceGeneratorInfo : IMapGeneratorInfo
	{
		public int ID => id;
		readonly int id;

		[Desc("NoiseMap that influences the placement of the piece. There will always be a little chance (10%) to ensure that the piece is placed.", "This setting only influences 'EXIT' and 'KEY' position modes.")]
		public readonly int NoiseMapID = -1;

		[Desc("Pieces of which one is chosen to be spawned.")]
		public readonly PackageFile[] Pieces = Array.Empty<PackageFile>();

		[Desc("Sets where to spawn the piece. This option only works if PositionType is set to 'POSITION'.")]
		public readonly MPos Position = MPos.Zero;

		[Desc("Position type.")]
		public readonly PositionType PositionType = PositionType.POSITION;
		[Desc("Determines when to generate this piece. If set to NONE, it will always be generated.")]
		public readonly ObjectiveType[] SpawnOnObjectives = new [] { ObjectiveType.NONE };

		[Desc("Allows this piece to be a waypoint as well. This means that any PathGenerator will take the piece under consideration for pathing.")]
		public readonly bool IsWaypoint = false;
		[Desc("Denies spawning patrols where the piece is situated.")]
		public readonly bool DenyPatrols = true;

		public ImportantPieceGeneratorInfo(int id, List<TextNode> nodes)
		{
			this.id = id;
			TypeLoader.SetValues(this, nodes);
		}

		public MapGenerator GetGenerator(Random random, MapLoader loader)
		{
			return new ImportantPieceGenerator(random, loader, this);
		}
	}

	public class ImportantPieceGenerator : MapGenerator
	{
		readonly ImportantPieceGeneratorInfo info;

		public ImportantPieceGenerator(Random random, MapLoader loader, ImportantPieceGeneratorInfo info) : base(random, loader)
		{
			this.info = info;
		}

		public override void Generate()
		{
			if (!info.SpawnOnObjectives.Contains(ObjectiveType.NONE) && !info.SpawnOnObjectives.Contains(Loader.ObjectiveType))
				return;

			var pieceIndex = info.Pieces[Random.Next(info.Pieces.Length)];
			var piece = PieceManager.GetPiece(pieceIndex);

			var noise = Loader.GetNoise(info.NoiseMapID);

			switch (info.PositionType)
			{
				case PositionType.POSITION:
					Loader.GenerateCrucialPiece(piece, info.Position, info.ID, info.DenyPatrols);
					markDirty(info.Position, piece);
					break;
				case PositionType.SPAWN:
					generateSpawn(piece);
					break;
				case PositionType.KEY:
					generateKey(piece, noise);
					break;
				case PositionType.EXIT:
					generateExit(piece, noise);
					break;
			}

			MapPrinter.PrintGeneratorMap(Bounds, noise, UsedCells, info.ID);
		}

		void generateSpawn(Piece piece)
		{
			var spawnArea = PlayableBounds - piece.Size;
			var eigth = spawnArea / 8;
			var pos = Center + new MPos(Random.Next(-eigth.X, eigth.X), Random.Next(-eigth.Y, eigth.Y));

			Loader.GenerateCrucialPiece(piece, pos, denyPatrols: info.DenyPatrols);
			Loader.PlayerSpawn = pos.ToCPos() + piece.Size.ToCPos() / 2;
			markDirty(pos, piece);

			if (info.IsWaypoint)
				addWaypoint(piece, pos);
		}

		void generateKey(Piece piece, NoiseMap noise)
		{
			var exitExists = Loader.Exit != CPos.Zero;
			var mapLength = PlayableBounds.Dist * 256;

			var dist = Random.Next(8);
			var spawnArea = PlayableBounds - (piece.Size + new MPos(dist, dist));

			MPos pos;
			WaypointLocation location;
			var success = false;
			do
			{
				pos = getPosNearBorder(spawnArea, out location);

				// Don't spawn near exits
				if (exitExists && (pos.ToCPos() - Loader.Exit).SquaredFlatDist < mapLength)
					continue;

				if (info.NoiseMapID >= 0 && Random.NextDouble() > noise[pos.X, pos.Y] + 0.1f)
					continue;

				success = Loader.GenerateCrucialPiece(piece, pos, denyPatrols: info.DenyPatrols);
				if (success)
					markDirty(pos, piece);
			}
			while (!success);

			Log.Debug($"(Maps) Generated key at {pos}");

			if (info.IsWaypoint)
				addWaypoint(piece, pos, location);
		}

		void generateExit(Piece piece, NoiseMap noise)
		{
			var spawnArea = PlayableBounds - piece.Size;

			MPos pos;
			WaypointLocation location;
			var success = false;
			do
			{
				pos = getPosNearBorder(spawnArea, out location);

				if (info.NoiseMapID >= 0 && Random.NextDouble() > noise[pos.X, pos.Y] + 0.1f)
					continue;

				success = Loader.GenerateCrucialPiece(piece, pos, denyPatrols: info.DenyPatrols);
				if (success)
					markDirty(pos, piece);
			}
			while (!success);

			Log.Debug($"(Maps) Generated exit at {pos}");

			Loader.Exit = pos.ToCPos() + piece.Size.ToCPos() / 2;

			if (info.IsWaypoint)
				addWaypoint(piece, pos, location);
		}

		void addWaypoint(Piece piece, MPos pos, WaypointLocation location = WaypointLocation.CENTER)
		{
			var position = pos + new MPos(piece.Size.X / 2, piece.Size.Y / 2);
			Loader.Waypoints.Add(new Waypoint(position, location, WaypointType.END));
		}

		MPos getPosNearBorder(MPos spawnArea, out WaypointLocation location)
		{
			var pos = MPos.Zero;
			location = WaypointLocation.CENTER;
			// Picking a random side, 0 = x, 1 = y, 2 = -x, 3 = -y;
			var side = (byte)Random.Next(4);
			switch (side)
			{
				case 0:
					pos = PlayableOffset + new MPos(Random.Next(2), Random.Next(spawnArea.X));
					location = WaypointLocation.TOP;
					break;
				case 1:
					pos = PlayableOffset + new MPos(Random.Next(spawnArea.Y), Random.Next(2));
					location = WaypointLocation.LEFT;
					break;
				case 2:
					pos = PlayableOffset + new MPos(spawnArea.X - Random.Next(2), Random.Next(spawnArea.X));
					location = WaypointLocation.BOTTOM;
					break;
				case 3:
					pos = PlayableOffset + new MPos(Random.Next(spawnArea.X), spawnArea.Y - Random.Next(2));
					location = WaypointLocation.RIGHT;
					break;
			}

			return pos;
		}

		void markDirty(MPos position, Piece piece)
		{
			for (int x = position.X; x < piece.Size.X + position.X; x++)
				for (int y = position.Y; y < piece.Size.Y + position.Y; y++)
					UsedCells[x, y] = true;
		}
	}
}
