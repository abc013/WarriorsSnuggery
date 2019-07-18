using System;

namespace WarriorsSnuggery.Maps
{
	public class ImportantPieceGenerator : MapGenerator
	{
		readonly ImportantPieceGeneratorInfo info;
		public ImportantPieceGenerator(Random random, Map map, World world, ImportantPieceGeneratorInfo info) : base(random, map, world)
		{
			this.info = info;
		}

		public override void Generate()
		{
			var pieceIndex = info.Pieces[random.Next(info.Pieces.Length)];
			var piece = Piece.LoadPiece(RuleReader.Read(FileExplorer.FindPath(FileExplorer.Maps, pieceIndex, ".yaml"), pieceIndex + ".yaml").ToArray());
			switch(info.PositionType)
			{
				case PositionType.POSITION:
					map.GeneratePiece(piece, info.Position, info.ID, true);
					break;
				case PositionType.SPAWN:
					spawnSpawn(piece);
					break;
				case PositionType.EXIT:
					exitSpawn(piece);
					break;
			}
		}

		void spawnSpawn(Piece piece)
		{
			var spawnArea = map.Bounds - piece.Size;
			var half = spawnArea / new MPos(2, 2);
			var eigth = spawnArea / new MPos(8, 8);
			var pos = half + new MPos(random.Next(eigth.X) - eigth.X / 2, random.Next(eigth.Y) - eigth.Y / 2);

			map.GeneratePiece(piece, pos, 100, true, true);
		}

		void exitSpawn(Piece piece)
		{
			var spawnArea = map.Bounds - piece.Size;
			var pos = new MPos(-1, -1);

			while (pos.X < 0 || !map.GeneratePiece(piece, pos, 0, true))
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

			map.Exit = pos + piece.Size / new MPos(2, 2);
		}

		protected override void MarkDirty()
		{
			throw new NotImplementedException();
		}

		protected override void DrawDirty()
		{
			throw new NotImplementedException();
		}

		protected override void ClearDirty()
		{
			throw new NotImplementedException();
		}
	}

	public enum PositionType
	{
		POSITION,
		SPAWN,
		EXIT
	}

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

		public ImportantPieceGeneratorInfo(int id, MiniTextNode[] nodes) : base(id)
		{
			ID = id;
			Loader.PartLoader.SetValues(this, nodes);
		}

		public override MapGenerator GetGenerator(Random random, Map map, World world)
		{
			return new ImportantPieceGenerator(random, map, world, this);
		}
	}
}
