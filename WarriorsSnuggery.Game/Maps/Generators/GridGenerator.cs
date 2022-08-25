using System;
using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Maps.Noises;
using WarriorsSnuggery.Maps.Pieces;

namespace WarriorsSnuggery.Maps.Generators
{
	[Desc("Generator used for generating grid-based towns or structures.")]
	public sealed class GridPiece
	{
		[Desc("Size of the piece in cells.")]
		public readonly MPos Size = MPos.Zero;

		[Desc("A selection of pieces. The game will choose a random one.")]
		public readonly PackageFile[] Pieces;

		public GridPiece(List<TextNode> nodes)
		{
			TypeLoader.SetValues(this, nodes);
		}
	}

	[Desc("Generator used for generating grid-based towns or structures.")]
	public sealed class GridGeneratorInfo : IMapGeneratorInfo
	{
		public int ID => id;
		readonly int id;

		[Desc("Size of the Cells in the grid.", "Must be a multiplex of GridSize.")]
		public readonly int CellSize = 4;
		[Desc("Size of the edges of the grid.")]
		public readonly int LineSize = 2;
		[Desc("Decides wether the grid is aligned rectangular or has random roads.")]
		public readonly bool Rectangular = true;

		public int Ratio => CellSize / LineSize;

		[Desc("Minimum dimensions of the grid in tiles.")]
		public readonly int MinimumDimensions = 16;
		[Desc("Maximum dimensions of the grid in tiles.")]
		public readonly int MaximumDimensions = 48;

		[Desc("Dimensions fit the map dimensions.")]
		public readonly bool FillMap = false;

		[Desc("Sets the Generator used for generating roads.", "Does not generate any roads when the ID is incorrect.")]
		public readonly int PathGeneratorID = 0;

		[Desc("List of various pieces to spawn with their size respectively.")]
		public readonly GridPiece[] Pieces = new GridPiece[0];

		public GridGeneratorInfo(int id, List<TextNode> nodes)
		{
			this.id = id;

			var fields = TypeLoader.GetFields(this);

			foreach (var node in nodes)
			{
				switch (node.Key)
				{
					case nameof(Pieces):
						Pieces = new GridPiece[node.Children.Count];
						for (int i = 0; i < node.Children.Count; i++)
						{
							var node2 = node.Children[i];
							Pieces[i] = new GridPiece(node2.Children);
						}

						break;
					default:
						TypeLoader.SetValue(this, fields, node);
						break;
				}
			}
		}

		public MapGenerator GetGenerator(Random random, MapLoader loader)
		{
			return new GridGenerator(random, loader, this);
		}
	}

	public class GridGenerator : MapGenerator
	{
		readonly GridGeneratorInfo info;

		readonly bool[,] road;
		List<Cell> cells;
		List<PieceCell> pieceCells;

		public GridGenerator(Random random, MapLoader loader, GridGeneratorInfo info) : base(random, loader)
		{
			this.info = info;

			road = new bool[Bounds.X, Bounds.Y];
		}

		public override void Generate()
		{
			var spawn = info.FillMap ? MPos.Zero : Center - new MPos(info.MinimumDimensions, info.MinimumDimensions);
			if (spawn.X < 0)
				spawn = new MPos(0, spawn.Y);

			if (spawn.Y < 0)
				spawn = new MPos(spawn.X, 0);

			var bounds = info.FillMap ? Bounds : new MPos(Random.Next(info.MaximumDimensions - info.MinimumDimensions) + info.MinimumDimensions, Random.Next(info.MaximumDimensions - info.MinimumDimensions) + info.MinimumDimensions);
			if (spawn.X + bounds.X >= Bounds.X)
				bounds = new MPos(Bounds.X - spawn.X, bounds.Y);

			if (spawn.Y + bounds.Y >= Bounds.Y)
				bounds = new MPos(bounds.X, Bounds.Y - spawn.Y);

			// If smaller than map bounds, abort
			if (bounds.X < info.MinimumDimensions || bounds.Y < info.MinimumDimensions)
				return;

			var baseCell = new Cell(16, spawn, bounds, info);
			cells = cellLoop(baseCell, 16);

			for (int x = spawn.X; x < spawn.X + bounds.X; x++)
				for (int y = spawn.Y; y < spawn.Y + bounds.Y; y++)
					road[x, y] = true;

			pieceCells = new List<PieceCell>();
			foreach (var cell in cells)
			{
				var piece = new PieceCell(cell.Position, cell.Size, info);
				pieceCells.AddRange(pieceLoop(piece));
			}

			foreach (var cell in cells)
			{
				if (!Loader.CanAcquireArea(cell.Position, cell.Size, info.ID))
					continue;

				for (int x = cell.Position.X; x < cell.Position.X + cell.Size.X; x++)
				{
					for (int y = cell.Position.Y; y < cell.Position.Y + cell.Size.Y; y++)
					{
						if (Loader.AcquireCell(new MPos(x, y), info.ID))
							UsedCells[x, y] = true;

						road[x, y] = false;
					}
				}
			}

			// Roads
			var type = Loader.Infos.FirstOrDefault(i => i.ID == info.PathGeneratorID && i is PathGeneratorInfo);
			if (type != null)
			{
				for (int x = 0; x < Bounds.X; x++)
				{
					for (int y = 0; y < Bounds.Y; y++)
					{
						if (!(road[x, y] && Loader.AcquireCell(new MPos(x, y), info.PathGeneratorID)))
							road[x, y] = false;
					}
				}
				var generator = new PathGenerator(Random, Loader, type as PathGeneratorInfo);
				generator.Generate(road);
			}
			// Pieces
			foreach (var piece in pieceCells)
			{
				var size = piece.Size / info.CellSize;

				var fitting = info.Pieces.FirstOrDefault(p => p.Size == size);

				if (fitting == null)
					continue;

				Loader.GeneratePiece(getPiece(fitting.Pieces), piece.Position, info.ID, idInclusive: true);
			}

			MapPrinter.PrintGeneratorMap(Bounds, NoiseMap.Empty, UsedCells, info.ID);
		}

		List<Cell> cellLoop(Cell cell, int depth)
		{
			var cells = new List<Cell>();

			if (depth > 0)
			{
				var childs = cell.Divide(Random, depth);

				if (childs == null)
					cells.Add(cell);
				else
				{
					foreach (var child in childs)
						cells.AddRange(cellLoop(child, depth - 1));
				}
			}
			else
				cells.Add(cell);

			return cells;
		}

		List<PieceCell> pieceLoop(PieceCell cell)
		{
			var cells = new List<PieceCell>();

			var childs = cell.Divide(Random);

			if (childs == null)
				cells.Add(cell);
			else
			{
				foreach (var child in childs)
					cells.AddRange(pieceLoop(child));
			}

			return cells;
		}

		Piece getPiece(PackageFile[] choices)
		{
			var choice = choices[Random.Next(choices.Length)];

			return PieceManager.GetPiece(choice);
		}
	}

	struct PieceCell
	{
		public readonly MPos Position;
		public readonly MPos Size;
		public readonly bool PieceAvailable;

		readonly GridGeneratorInfo info;
		bool divideHorizontal;

		public PieceCell(MPos position, MPos size, GridGeneratorInfo info, bool divideHorizontal = true)
		{
			Position = position;
			Size = size;

			this.info = info;
			this.divideHorizontal = divideHorizontal;

			PieceAvailable = size.X <= 2 * info.CellSize && size.Y <= 2 * info.CellSize;
		}

		public PieceCell[] Divide(Random random, bool secondTry = false)
		{
			if (PieceAvailable && random.NextDouble() < 0.8f)
				return null;

			var cellCount = (divideHorizontal ? Size.Y : Size.X) / info.CellSize;

			if (cellCount < 2)
			{
				if (secondTry)
					return null;

				divideHorizontal = !divideHorizontal;
				return Divide(random, true);
			}

			var cells = new PieceCell[2];

			var cells1 = random.Next(cellCount - 1) + 1;
			var cells2 = cellCount - cells1;

			if (divideHorizontal)
			{
				cells[0] = new PieceCell(Position, new MPos(Size.X, cells1 * info.CellSize), info, !divideHorizontal);
				// Remove cell above and the path inbetween
				cells[1] = new PieceCell(Position + new MPos(0, cells1 * info.CellSize), new MPos(Size.X, cells2 * info.CellSize), info, !divideHorizontal);
			}
			else
			{
				cells[0] = new PieceCell(Position, new MPos(cells1 * info.CellSize, Size.Y), info);
				// Remove cell above and the path inbetween
				cells[1] = new PieceCell(Position + new MPos(cells1 * info.CellSize, 0), new MPos(cells2 * info.CellSize, Size.Y), info, !divideHorizontal);
			}

			return cells;
		}
	}

	struct Cell
	{
		public readonly int Depth;

		public readonly MPos Position;
		public readonly MPos Size;

		readonly GridGeneratorInfo info;
		bool divideHorizontal;

		public Cell(int depth, MPos position, MPos size, GridGeneratorInfo info, bool divideHorizontal = true)
		{
			Depth = depth;
			Position = position;
			Size = size;

			this.info = info;
			this.divideHorizontal = divideHorizontal;
		}

		public Cell[] Divide(Random random, int depth, bool otherSidetried = false)
		{
			var cellSize = info.CellSize;
			var lineSize = info.LineSize;

			int length = divideHorizontal ? Size.Y : Size.X;

			if (length >= 2 * cellSize + lineSize)
			{
				var cellCount = (int)Math.Floor(length / (float)cellSize);

				// Should be always divideable, remove one as it will be established as path
				var pathCount = (length % cellSize / lineSize) - 1;

				// Sometimes, do not divide any further
				//if (cellsPossible < 4 && paths < 0 && random.NextDouble() < 0.2f)
				//{
				//	if (!otherSidetried && random.NextDouble() < 0.7f)
				//	{
				//		divideHorizontal = !divideHorizontal;
				//		return Divide(random, depth, true);
				//	}
				//	else
				//	{
				//		return null;
				//	}
				//}

				//If there are no paths currently and the following fields are too small to get paths inbetween, dont divide
				if (pathCount < 0 && (cellCount - 1) <= info.Ratio)
				{
					if (otherSidetried)
						return null;

					divideHorizontal = !divideHorizontal;
					return Divide(random, depth, true);
				}
				else if (pathCount < 0)
				{
					cellCount--;
					pathCount += info.Ratio;
				}

				var cells = new Cell[2];

				var cells1 = random.Next(cellCount - 1) + 1; // Ideal would be of course: 1-3, maximum is cellsPossible - 1 cause the other cells needs to be minimum 1
				var cells2 = cellCount - cells1;

				int paths1;
				int paths2;
				if (cells1 < cells2)
				{
					paths1 = random.Next(cells1 - 1);
					paths2 = pathCount - paths1;
				}
				else
				{
					paths2 = random.Next(cells2 - 1);
					paths1 = pathCount - paths2;
				}

				if (divideHorizontal)
				{
					cells[0] = new Cell(depth, Position, new MPos(Size.X, cells1 * cellSize + paths1 * lineSize), info, !divideHorizontal);
					// Remove cell above and the path inbetween
					cells[1] = new Cell(depth, Position + new MPos(0, cells1 * cellSize + paths1 * lineSize + lineSize), new MPos(Size.X, cells2 * cellSize + paths2 * lineSize), info, !divideHorizontal);
				}
				else
				{
					cells[0] = new Cell(depth, Position, new MPos(cells1 * cellSize + paths1 * lineSize, Size.Y), info);
					// Remove cell above and the path inbetween
					cells[1] = new Cell(depth, Position + new MPos(cells1 * cellSize + paths1 * lineSize + lineSize, 0), new MPos(cells2 * cellSize + paths2 * lineSize, Size.Y), info, !divideHorizontal);
				}

				return cells;
			}
			else if (!otherSidetried)
			{
				divideHorizontal = !divideHorizontal;
				return Divide(random, depth, true);
			}

			return null;
		}
	}
}
