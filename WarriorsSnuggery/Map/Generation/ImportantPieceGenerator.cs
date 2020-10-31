using System;
using System.Collections.Generic;

namespace WarriorsSnuggery.Maps
{
	public enum PositionType
	{
		POSITION,
		SPAWN,
		EXIT
	}

	[Desc("Generator used to generate pieces that must be on the map.")]
	public class ImportantPieceGeneratorInfo : MapGeneratorInfo
	{
		[Desc("Unique ID for the generator.")]
		public readonly new int ID;

		[Desc("Pieces of which one is chosen to be spawned.")]
		public readonly string[] Pieces = new string[0];

		[Desc("Position on the map, if PositionType is set on 'POSITION'.")]
		public readonly MPos Position = MPos.Zero;
		[Desc("Position type.")]
		public readonly PositionType PositionType = PositionType.POSITION;

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

		public Piece Piece;
		public ImportantPieceGenerator(Random random, MapLoader loader, ImportantPieceGeneratorInfo info) : base(random, loader)
		{
			this.info = info;
		}

		public override void Generate()
		{
			var pieceIndex = info.Pieces[random.Next(info.Pieces.Length)];
			Piece = PieceManager.GetPiece(pieceIndex);
			switch (info.PositionType)
			{
				case PositionType.POSITION:
					loader.GenerateCrucialPiece(Piece, info.Position, info.ID);
					break;
				case PositionType.SPAWN:
					spawnSpawn();
					break;
				case PositionType.EXIT:
					if (loader.GameMode == GameMode.FIND_EXIT)
						exitSpawn();
					break;
			}
		}

		void spawnSpawn()
		{
			var spawnArea = Bounds - Piece.Size;
			var half = spawnArea / new MPos(2, 2);
			var eigth = spawnArea / new MPos(8, 8);
			var pos = half + new MPos(random.Next(eigth.X) - eigth.X / 2, random.Next(eigth.Y) - eigth.Y / 2);

			loader.GenerateCrucialPiece(Piece, pos);
			loader.PlayerSpawn = new CPos(pos.X * 1024 + Piece.Size.X * 512, pos.Y * 1024 + Piece.Size.Y * 512, 0);
		}

		void exitSpawn()
		{
			var spawnArea = Bounds - Piece.Size;
			var pos = new MPos(-1, -1);

			while (pos.X < 0 || !loader.GeneratePiece(Piece, pos, 100, true))
			{
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
			}

			loader.Exit = pos.ToCPos() + new CPos(Piece.Size.X * 512, Piece.Size.Y * 512, 0);
		}
	}
}
