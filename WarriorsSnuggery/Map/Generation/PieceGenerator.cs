using System;
using System.Collections.Generic;
using System.Linq;

namespace WarriorsSnuggery.Maps.Generators
{
	[Desc("Generator used to randomly spread pieces across the map.", "It is not guaranteed that the MinimumCount can be fullfilled.")]
	public class PieceGeneratorInfo : IMapGeneratorInfo
	{
		public int ID => id;
		readonly int id;

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

		public PieceGeneratorInfo(int id, List<MiniTextNode> nodes)
		{
			this.id = id;
			Loader.PartLoader.SetValues(this, nodes);
		}

		public MapGenerator GetGenerator(Random random, MapLoader loader)
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
			var noise = GeneratorUtils.GetNoise(Loader, info.NoiseMapID);

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

						var value = noise[x, y];
						if (info.NoiseMapLimit > value)
						{
							canAcquire = false;
							break;
						}

						if (!Loader.CanAcquireCell(new MPos(x, y), info.ID))
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

							if (!Loader.CanAcquireCell(new MPos(x, y), info.ID))
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

							if (!Loader.CanAcquireCell(new MPos(x, y), info.ID))
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
			var count = Random.Next((int)(info.MinimumCount * multiplier), (int)(info.MaximumCount * multiplier));
			for (int i = 0; i < count; i++)
			{
				if (!possiblePlaces.Any())
					break;

				var piece = info.Pieces[Random.Next(info.Pieces.Length)];
				var input = PieceManager.GetPiece(piece);

				var position = Random.Next(possiblePlaces.Count);

				if (!Loader.GeneratePiece(input, possiblePlaces[position], info.ID, idInclusive: true))
					i--;
				else
					markDirty(possiblePlaces[position], input);

				possiblePlaces.RemoveAt(position);
			}

			MapPrinter.PrintGeneratorMap(Bounds, noise, UsedCells, info.ID);
		}

		void markDirty(MPos position, Piece piece)
		{
			for (int x = position.X; x < piece.Size.X + position.X; x++)
				for (int y = position.Y; y < piece.Size.Y + position.Y; y++)
					UsedCells[x, y] = true;
		}
	}
}
