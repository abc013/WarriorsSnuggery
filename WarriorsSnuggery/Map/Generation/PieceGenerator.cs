using System;

namespace WarriorsSnuggery.Maps
{
	public class PieceGenerator : MapGenerator
	{
		readonly PieceGeneratorInfo info;
		public PieceGenerator(Random random, Map map, World world, PieceGeneratorInfo info) : base(random, map, world)
		{
			this.info = info;
		}

		public override void Generate()
		{
			var maxLoops = 2000;
			var count = random.Next(info.MaximumCount - info.MinimumCount) + info.MinimumCount;

			while(count > 0)
			{
				var piece = info.Pieces[random.Next(info.Pieces.Length)];
				var input = RuleReader.Read(FileExplorer.FindPath(FileExplorer.Maps, piece, ".yaml"), piece + ".yaml").ToArray();

				var pos = MapUtils.RandomPositionInMap(random, 10, map.Bounds);

				if (map.GeneratePiece(input, pos, info.ID, cancelIfAcquiredBySameID: true))
				{
					count--;
				}
				else if (maxLoops-- <= 0)
				{
					Log.WriteDebug(string.Format("Unable to generate desired count of pieces ({0} pieces left).", count));
					break;
				}
			}
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

	public class PieceGeneratorInfo : MapGeneratorInfo
	{
		[Desc("Unique ID for the generator.")]
		public readonly new int ID;

		[Desc("Pieces to possibly spawn.")]
		public readonly string[] Pieces = new string[0];

		[Desc("Minimum count of pieces on the map.")]
		public readonly int MinimumCount;
		[Desc("Maximum count of pieces on the map.")]
		public readonly int MaximumCount;

		public PieceGeneratorInfo(int id, MiniTextNode[] nodes) : base(id)
		{
			ID = id;
			Loader.PartLoader.SetValues(this, nodes);
		}

		public override MapGenerator GetGenerator(Random random, Map map, World world)
		{
			return new PieceGenerator(random, map, world, this);
		}
	}
}
