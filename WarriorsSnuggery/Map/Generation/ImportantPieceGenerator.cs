using System;
using System.Collections.Generic;
using System.Linq;

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
	public class ImportantPieceGeneratorInfo : MapGeneratorInfo
	{
		[Desc("Unique ID for the generator.")]
		public readonly new int ID;

		[Desc("NoiseMap that influences the placement of the piece. There will always be a little chance (10%) to ensure that the piece is placed.", "This setting only influences 'EXIT' and 'KEY' position modes.")]
		public readonly int NoiseMapID = -1;

		[Desc("Pieces of which one is chosen to be spawned.")]
		public readonly string[] Pieces = new string[0];

		[Desc("Sets where to spawn the piece. This option only works if PositionType is set to 'POSITION'.")]
		public readonly MPos Position = MPos.Zero;

		[Desc("Position type.")]
		public readonly PositionType PositionType = PositionType.POSITION;
		[Desc("Determines when to generate this piece. If set to NONE, it will always be generated.")]
		public readonly ObjectiveType[] SpawnOnObjectives = new [] { ObjectiveType.NONE };

		public ImportantPieceGeneratorInfo(int id, List<MiniTextNode> nodes) : base(id)
		{
			ID = id;
			Loader.PartLoader.SetValues(this, nodes);
		}

		public override MapGenerator GetGenerator(Random random, MapLoader loader)
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
			if (!info.SpawnOnObjectives.Contains(ObjectiveType.NONE) && !info.SpawnOnObjectives.Contains(loader.ObjectiveType))
				return;

			var pieceIndex = info.Pieces[random.Next(info.Pieces.Length)];
			var piece = PieceManager.GetPiece(pieceIndex);

			var noise = GeneratorUtils.GetNoise(loader, info.NoiseMapID);

			switch (info.PositionType)
			{
				case PositionType.POSITION:
					loader.GenerateCrucialPiece(piece, info.Position, info.ID);
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

			MapPrinter.PrintGeneratorMap(Bounds, noise, dirtyCells, info.ID);
		}

		void generateSpawn(Piece piece)
		{
			var spawnArea = Bounds - piece.Size;
			var half = spawnArea / new MPos(2, 2);
			var eigth = spawnArea / new MPos(8, 8);
			var pos = half + new MPos(random.Next(eigth.X) - eigth.X / 2, random.Next(eigth.Y) - eigth.Y / 2);

			loader.GenerateCrucialPiece(piece, pos);
			loader.PlayerSpawn = new CPos(pos.X * 1024 + piece.Size.X * 512, pos.Y * 1024 + piece.Size.Y * 512, 0);
			markDirty(pos, piece);
		}

		void generateKey(Piece piece, NoiseMap noise)
		{
			var exitExists = loader.Exit == CPos.Zero;
			var mapLength = Bounds.Dist * 256;

			var dist = random.Next(8);
			var spawnArea = Bounds - (piece.Size + new MPos(dist, dist));

			MPos pos;
			var success = false;
			do
			{
				pos = getPosNearBorder(spawnArea);

				// Don't spawn near exits
				if (exitExists && (pos.ToCPos() - loader.Exit).SquaredFlatDist < mapLength)
					continue;

				if (info.NoiseMapID >= 0 && random.NextDouble() > noise[pos.X, pos.Y] + 0.1f)
					continue;

				success = loader.GenerateCrucialPiece(piece, pos);
				if (success)
					markDirty(pos, piece);
			}
			while (!success);
		}

		void generateExit(Piece piece, NoiseMap noise)
		{
			var spawnArea = Bounds - piece.Size;

			MPos pos;
			var success = false;
			do
			{
				pos = getPosNearBorder(spawnArea);

				if (info.NoiseMapID >= 0 && random.NextDouble() > noise[pos.X, pos.Y] + 0.1f)
					continue;

				success = loader.GenerateCrucialPiece(piece, pos);
				if (success)
					markDirty(pos, piece);
			}
			while (!success);

			loader.Exit = pos.ToCPos() + new CPos(piece.Size.X * 512, piece.Size.Y * 512, 0);
		}

		MPos getPosNearBorder(MPos spawnArea)
		{
			var pos = MPos.Zero;
			// Picking a random side, 0 = x, 1 = y, 2 = -x, 3 = -y;
			var side = (byte)random.Next(4);
			switch (side)
			{
				case 0:
					pos = new MPos(random.Next(2), random.Next(spawnArea.X));
					break;
				case 1:
					pos = new MPos(random.Next(spawnArea.Y), random.Next(2));
					break;
				case 2:
					pos = new MPos(spawnArea.X - random.Next(2), random.Next(spawnArea.X));
					break;
				case 3:
					pos = new MPos(random.Next(spawnArea.X), spawnArea.Y - random.Next(2));
					break;
			}

			return pos;
		}

		void markDirty(MPos position, Piece piece)
		{
			for (int x = position.X; x < piece.Size.X + position.X; x++)
				for (int y = position.Y; y < piece.Size.Y + position.Y; y++)
					dirtyCells[x, y] = true;
		}
	}
}
