using System;
using System.Collections.Generic;
using System.Linq;

namespace WarriorsSnuggery.Maps
{
	[Desc("Generator used to randomly spread pieces across the map.", "It is not guaranteed that the MinimumCount can be fullfilled.")]
	public class PieceGeneratorInfo : MapGeneratorInfo
	{
		[Desc("Unique ID for the generator.")]
		public readonly new int ID;

		[Desc("NoiseMap used to find a spot where the piece is generated.")]
		public readonly int NoiseMapID = -1;

		[Desc("All numbers above this limit in the NoiseMap can be used for placing a piece.")]
		public readonly float NoiseMapLimit = 0;

		[Desc("Pieces to possibly spawn.")]
		public readonly string[] Pieces = new string[0];

		[Desc("maximum bounds for finer piecespawning.", "If set to zero, size will be determined automatically.")]
		public readonly MPos MaxBounds = new MPos(0, 0);
		[Desc("Searches for more spawning opportunities by shifting.", "The lower the value, the finer the search.")]
		public readonly int ShiftStep = 2;

		[Desc("Minimum count of pieces on the map per 32x32 field.")]
		public readonly int MinimumCount = 1;
		[Desc("Maximum count of pieces on the map per 32x32 field.")]
		public readonly int MaximumCount = 4;

		public PieceGeneratorInfo(int id, List<MiniTextNode> nodes) : base(id)
		{
			ID = id;
			Loader.PartLoader.SetValues(this, nodes);
		}

		public override MapGenerator GetGenerator(Random random, MapLoader loader)
		{
			return new PieceGenerator(random, loader, this);
		}
	}

	public class PieceGenerator : MapGenerator
	{
		readonly PieceGeneratorInfo info;

		MPos searchBlocks;
		MPos searchBounds;
		int positions;
		readonly List<MPos> possiblePlaces = new List<MPos>();

		public PieceGenerator(Random random, MapLoader loader, PieceGeneratorInfo info) : base(random, loader)
		{
			this.info = info;
		}

		public override void Generate()
		{
			var noise = GeneratorUtils.GetNoise(loader, info.NoiseMapID);

			searchBlocks = info.MaxBounds;
			if (searchBlocks == MPos.Zero)
			{
				var blockSizes = new MPos[info.Pieces.Length];
				for (int i = 0; i < blockSizes.Length; i++)
					blockSizes[i] = PieceManager.GetPiece(info.Pieces[i]).Size;

				var x = blockSizes.Max(b => b.X);
				var y = blockSizes.Max(b => b.Y);
				searchBlocks = new MPos(x, y);
			}
			searchBounds = new MPos((int)Math.Floor(Bounds.X / (float)searchBlocks.X), (int)Math.Floor(Bounds.Y / (float)searchBlocks.Y));
			positions = searchBounds.X * searchBounds.Y;

			// usual grid
			for (int i = 0; i < positions; i++)
			{
				var xStart = (i % searchBounds.X) * searchBounds.X;
				if (xStart >= Bounds.X)
					break;

				var yStart = (int)Math.Floor(i / (float)searchBounds.X) * searchBounds.Y;

				var canAcquire = true;
				for (var x = xStart; x < xStart + searchBlocks.X; x++)
				{
					if (x >= Bounds.X)
						break;

					if (!canAcquire)
						break;

					for (var y = yStart; y < yStart + searchBlocks.Y; y++)
					{
						if (y >= Bounds.Y)
							break;

						var value = noise[y * Bounds.X + x];
						if (info.NoiseMapLimit > value)
						{
							canAcquire = false;
							break;
						}

						if (!loader.CanAcquireCell(new MPos(x, y), info.ID))
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
					if (xStart >= Bounds.X)
						break;

					var yStart = (int)Math.Floor(i / (float)searchBounds.X) * searchBounds.Y;

					var canAcquire = true;
					for (var x = xStart; x < xStart + searchBlocks.X; x++)
					{
						if (x >= Bounds.X)
							break;

						if (!canAcquire)
							break;

						for (var y = yStart; y < yStart + searchBlocks.Y; y++)
						{
							if (y >= Bounds.Y)
								break;

							if (!loader.CanAcquireCell(new MPos(x, y), info.ID))
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
					if (yStart >= Bounds.Y)
						break;

					var canAcquire = true;
					for (var x = xStart; x < xStart + searchBlocks.X; x++)
					{
						if (x >= Bounds.X)
							break;

						if (!canAcquire)
							break;

						for (var y = yStart; y < yStart + searchBlocks.Y; y++)
						{
							if (y >= Bounds.Y)
								break;

							if (!loader.CanAcquireCell(new MPos(x, y), info.ID))
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

			var multiplier = Bounds.X * Bounds.Y / (float)(32 * 32);
			var count = random.Next((int)(info.MinimumCount * multiplier), (int)(info.MaximumCount * multiplier));
			for (int i = 0; i < count; i++)
			{
				if (!possiblePlaces.Any())
					break;

				var piece = info.Pieces[random.Next(info.Pieces.Length)];
				var input = PieceManager.GetPiece(piece);

				var position = random.Next(possiblePlaces.Count);

				if (!loader.GeneratePiece(input, possiblePlaces[position], info.ID, cancelIfAcquiredBySameID: true))
					i--;

				possiblePlaces.RemoveAt(position);
			}
		}
	}
}
