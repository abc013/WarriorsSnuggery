using System;
using System.Collections.Generic;
using System.Linq;

namespace WarriorsSnuggery.Maps
{
	public class PieceGenerator : MapGenerator
	{
		readonly PieceGeneratorInfo info;

		MPos searchBlocks;
		MPos searchBounds;
		int positions;
		readonly List<MPos> possiblePlaces = new List<MPos>();

		public PieceGenerator(Random random, Map map, World world, PieceGeneratorInfo info) : base(random, map, world)
		{
			this.info = info;
		}

		public override void Generate()
		{
			searchBlocks = info.MaxBounds;
			if (searchBlocks == MPos.Zero)
			{
				var blockSizes = new MPos[info.Pieces.Length];
				for (int i = 0; i < blockSizes.Length; i++)
				{
					blockSizes[i] = RuleReader.Read(FileExplorer.FindPath(FileExplorer.Maps, info.Pieces[i], ".yaml"), info.Pieces[i] + ".yaml").Find(n => n.Key == "Size").Convert<MPos>();
				}
				var x = blockSizes.Max(b => b.X);
				var y = blockSizes.Max(b => b.Y);
				searchBlocks = new MPos(x, y);
			}
			searchBounds = new MPos((int)Math.Floor(map.Bounds.X / (float)searchBlocks.X), (int)Math.Floor(map.Bounds.Y / (float)searchBlocks.Y));
			positions = searchBounds.X * searchBounds.Y;

			MarkDirty();
			DrawDirty();
			ClearDirty();
		}

		protected override void MarkDirty()
		{
			// usual grid
			for (int i = 0; i < positions; i++)
			{
				var xStart = (i % searchBounds.X) * searchBounds.X;
				if (xStart >= map.Bounds.X)
					break;

				var yStart = (int)Math.Floor(i / (float)searchBounds.X) * searchBounds.Y;

				var canAcquire = true;
				for (var x = xStart; x < xStart + searchBlocks.X; x++)
				{
					if (!canAcquire)
						break;

					for (var y = yStart; y < yStart + searchBlocks.Y; y++)
					{
						if (!map.CanAcquireCell(new MPos(x, y), info.ID))
						{
							canAcquire = false;
							break;
						}
					}
				}

				if (canAcquire)
					possiblePlaces.Add(new MPos(xStart, yStart));
			}

			// Additional refining using steps
			for (int step = 0; step < searchBounds.X; step += info.ShiftStep)
			{
				for (int i = 0; i < positions; i++)
				{
					var xStart = (i % searchBounds.X) * searchBounds.X + step;
					if (xStart >= map.Bounds.X)
						break;

					var yStart = (int)Math.Floor(i / (float)searchBounds.X) * searchBounds.Y;

					var canAcquire = true;
					for (var x = xStart; x < xStart + searchBlocks.X; x++)
					{
						if (!canAcquire)
							break;

						for (var y = yStart; y < yStart + searchBlocks.Y; y++)
						{
							if (!map.CanAcquireCell(new MPos(x, y), info.ID))
							{
								canAcquire = false;
								break;
							}
						}
					}

					if (canAcquire)
						possiblePlaces.Add(new MPos(xStart, yStart));
				}
			}
			for (int step = 0; step < searchBounds.Y; step += info.ShiftStep)
			{
				for (int i = 0; i < positions; i++)
				{
					var xStart = (i % searchBounds.X) * searchBounds.X;
					var yStart = (int)Math.Floor(i / (float)searchBounds.X) * searchBounds.Y + step;
					if (yStart >= map.Bounds.Y)
						break;

					var canAcquire = true;
					for (var x = xStart; x < xStart + searchBlocks.X; x++)
					{
						if (!canAcquire)
							break;

						for (var y = yStart; y < yStart + searchBlocks.Y; y++)
						{
							if (!map.CanAcquireCell(new MPos(x, y), info.ID))
							{
								canAcquire = false;
								break;
							}
						}
					}

					if (canAcquire)
						possiblePlaces.Add(new MPos(xStart, yStart));
				}
			}
		}

		protected override void DrawDirty()
		{
			var count = random.Next(info.MinimumCount, info.MaximumCount);
			for (int i = 0; i < count; i++)
			{
				if (!possiblePlaces.Any())
					break;

				var piece = info.Pieces[random.Next(info.Pieces.Length)];
				var input = RuleReader.Read(FileExplorer.FindPath(FileExplorer.Maps, piece, ".yaml"), piece + ".yaml").ToArray();

				var position = random.Next(possiblePlaces.Count);

				if (!map.GeneratePiece(input, possiblePlaces[position], info.ID, cancelIfAcquiredBySameID: true))
					i--;

				possiblePlaces.RemoveAt(position);
			}
		}

		protected override void ClearDirty()
		{
			searchBlocks = MPos.Zero;
			possiblePlaces.Clear();
			positions = 0;
		}
	}

	[Desc("Generator used to randomly spread pieces across the map.", "It is not guaranteed that the MinimumCount can be fullfilled.")]
	public class PieceGeneratorInfo : MapGeneratorInfo
	{
		[Desc("Unique ID for the generator.")]
		public readonly new int ID;

		[Desc("Pieces to possibly spawn.")]
		public readonly string[] Pieces = new string[0];

		[Desc("maximum bounds for finer piecespawning.", "If set to zero, size will be determined automatically.")]
		public readonly MPos MaxBounds = new MPos(0, 0);
		[Desc("Searches for more spawning opportunities by shifting.", "The lower the value, the finer the search.")]
		public readonly int ShiftStep = 2;

		[Desc("Minimum count of pieces on the map.")]
		public readonly int MinimumCount = 1;
		[Desc("Maximum count of pieces on the map.")]
		public readonly int MaximumCount = 4;

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
