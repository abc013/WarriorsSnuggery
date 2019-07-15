using System;
using System.Linq;
using System.Collections.Generic;

namespace WarriorsSnuggery.Maps
{
	public class GridGenerator : MapGenerator
	{
		readonly GridGeneratorInfo info;

		bool[,] road;
		List<Cell> cells;
		List<PieceCell> pieceCells;

		public GridGenerator(Random random, Map map, World world, GridGeneratorInfo info) : base(random, map, world)
		{
			this.info = info;

			road = new bool[map.Bounds.X, map.Bounds.Y];
		}

		public override void Generate()
		{
			var spawn = info.FillMap ? MPos.Zero : map.Center - new MPos(info.MinimumDimensions, info.MinimumDimensions);
			if (spawn.X < 0)
			{
				spawn = new MPos(0, spawn.Y);
			}
			if (spawn.Y < 0)
			{
				spawn = new MPos(spawn.X, 0);
			}
			// TODO use Minimum and Maximum
			var bounds = info.FillMap ? map.Bounds : new MPos(random.Next(info.MaximumDimensions - info.MinimumDimensions) + info.MinimumDimensions, random.Next(info.MaximumDimensions - info.MinimumDimensions) + info.MinimumDimensions);
			if (spawn.X + bounds.X >= map.Bounds.X)
			{
				bounds = new MPos(map.Bounds.X - spawn.X, bounds.Y);
			}
			if (spawn.Y + bounds.Y >= map.Bounds.Y)
			{
				bounds = new MPos(bounds.X, map.Bounds.Y - spawn.Y);
			}

			// If smaller than map bounds, abort
			if (bounds.X < info.MinimumDimensions || bounds.Y < info.MinimumDimensions)
				return;

			var baseCell = new Cell(16, spawn, bounds, info);
			cells = cellLoop(baseCell, 16);

			for (int x = spawn.X; x < spawn.X + bounds.X; x++)
			{
				for (int y = spawn.Y; y < spawn.Y + bounds.Y; y++)
				{
					road[x, y] = true;
				}
			}

			pieceCells = new List<PieceCell>();
			foreach(var cell in cells)
			{
				var piece = new PieceCell(cell.Position, cell.Size, info);
				pieceCells.AddRange(pieceLoop(piece));
			}

			MarkDirty();
			DrawDirty();
			ClearDirty();
		}

		List<Cell> cellLoop(Cell cell, int depth)
		{
			var cells = new List<Cell>();

			if (depth > 0)
			{
				var childs = cell.Divide(random, depth);

				if (childs == null)
					cells.Add(cell);
				else
				{
					foreach (var child in childs)
					{
						cells.AddRange(cellLoop(child, depth - 1));
					}
				}
			}
			else
			{
				cells.Add(cell);
			}

			return cells;
		}
		List<PieceCell> pieceLoop(PieceCell cell)
		{
			var cells = new List<PieceCell>();

			var childs = cell.Divide(random);

			if (childs == null)
				cells.Add(cell);
			else
			{
				foreach (var child in childs)
				{
					cells.AddRange(pieceLoop(child));
				}
			}

			return cells;
		}

		protected override void MarkDirty()
		{
			foreach(var cell in cells)
			{
				for(int x = cell.Position.X; x < cell.Position.X + cell.Size.X; x++)
				{
					for (int y = cell.Position.Y; y < cell.Position.Y + cell.Size.Y; y++)
					{
						if (map.AcquireCell(new MPos(x, y), info.ID))
						{
							dirtyCells[x, y] = true;
						}
						road[x, y] = false;
					}
				}
			}
		}

		protected override void DrawDirty()
		{
			// Roads
			var type = map.Type.GeneratorInfos.Where(i => i.ID == info.PathGeneratorID && i is PathGeneratorInfo).FirstOrDefault();
			if (type != null)
			{
				var generator = new PathGenerator(random, map, world, type as PathGeneratorInfo);
				generator.Generate(road);
			}
			// Pieces
			foreach (var piece in pieceCells)
			{
				MiniTextNode[] toUse;
				var x = piece.Size.X / info.CellSize;
				var y = piece.Size.Y / info.CellSize;

				// TODO also allow tiles bigger than 2x2
				if (x == 1 && y == 1)
					toUse = getPiece(info.Tile1x1);
				else if (x == 1 && y == 2)
					toUse = getPiece(info.Tile1x2);
				else if (x == 2 && y == 1)
					toUse = getPiece(info.Tile2x1);
				else
					toUse = getPiece(info.Tile2x2);

				map.LoadPiece(toUse, piece.Position, info.ID);
			}
		}

		MiniTextNode[] getPiece(string[] choices)
		{
			var choice = choices[random.Next(choices.Length)];

			return RuleReader.Read(FileExplorer.FindPath(FileExplorer.Maps, choice, ".yaml"), choice + ".yaml").ToArray();
		}

		protected override void ClearDirty()
		{
			dirtyCells = new bool[map.Bounds.X, map.Bounds.Y];
			road = new bool[map.Bounds.X, map.Bounds.Y];

			cells.Clear();
			pieceCells.Clear();
		}
	}

	[Desc("Generator used for generating grid-based towns or structures.")]
	public sealed class GridGeneratorInfo : MapGeneratorInfo
	{
		[Desc("Unique ID for the generator.")]
		public readonly new int ID;

		[Desc("Size of the Cells in the grid.", "Must be a multiplex of GridSize.")]
		public readonly int CellSize = 4;
		[Desc("Size of the edges of the grid.")]
		public readonly int LineSize = 2;
		[Desc("Decides wether the grid is aligned rectangular or has random roads.")]
		public readonly bool Rectangular = true;

		public int Ratio
		{
			get
			{
				return CellSize / LineSize;
			}
			set
			{

			}
		}

		[Desc("Minimum dimensions of the grid in tiles.")]
		public readonly int MinimumDimensions = 16;
		[Desc("Maximum dimensions of the grid in tiles.")]
		public readonly int MaximumDimensions = 64;

		[Desc("Dimensions fit the map dimensions.")]
		public readonly bool FillMap = false;

		[Desc("Sets the Generator used for generating roads.", "Does not generate any roads when the ID is incorrect.")]
		public readonly int PathGeneratorID = 0;

		[Desc("1x1 Dimension tiles.")]
		public readonly string[] Tile1x1;
		[Desc("2x1 Dimension tiles.")]
		public readonly string[] Tile2x1;
		[Desc("1x2 Dimension tiles.")]
		public readonly string[] Tile1x2;
		[Desc("2x2 Dimension tiles.")]
		public readonly string[] Tile2x2;

		public GridGeneratorInfo(int id, MiniTextNode[] nodes) : base(id)
		{
			ID = id;
			Loader.PartLoader.SetValues(this, nodes);
		}

		public override MapGenerator GetGenerator(Random random, Map map, World world)
		{
			return new GridGenerator(random, map, world, this);
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
				{
					return null;
				}
				else
				{
					divideHorizontal = !divideHorizontal;
					return Divide(random, true);
				}
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
					if (!otherSidetried)
					{
						divideHorizontal = !divideHorizontal;
						return Divide(random, depth, true);
					}
					else
					{
						return null;
					}
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
