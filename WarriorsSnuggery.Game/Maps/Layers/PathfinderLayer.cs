﻿using System;
using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.Maps.Layers
{
	public sealed class PathfinderLayer
	{
		const float movementCostFactor = 100f;

		readonly PathfinderCell[,] cells;
		readonly MPos bounds;

		public PathfinderLayer(MPos bounds)
		{
			this.bounds = bounds;
			cells = new PathfinderCell[bounds.X, bounds.Y];
		}

		public void Update(WallLayer wallLayer, TerrainLayer terrainLayer)
		{
			// Generate navigatable mesh

			for (int x = 0; x < bounds.X; x++)
			{
				for (int y = 0; y < bounds.Y; y++)
				{
					var cell = new PathfinderCell(new MPos(x, y))
					{
						TerrainCost = 1f / terrainLayer.Terrain[x, y].Type.Speed
					};

					cells[x, y] = cell;
				}
			}

			static void connect(Wall between, PathfinderCell a, PathfinderCell b)
			{
				if (between != null && between.Type.Blocks)
					return;

				if (float.IsInfinity(a.TerrainCost) || float.IsInfinity(b.TerrainCost))
					return;

				var cost = (a.TerrainCost + b.TerrainCost) / 2;
				a.Connections.Add((cost, b));
				b.Connections.Add((cost, a));
			};

			for (int x = 0; x < bounds.X; x++)
			{
				for (int y = 0; y < bounds.Y; y++)
				{
					if (x < bounds.X - 1)
						connect(wallLayer.Walls[(x + 1) * 2, y], cells[x, y], cells[x + 1, y]);

					if (y < bounds.Y - 1)
						connect(wallLayer.Walls[x * 2 + 1, y + 1], cells[x, y], cells[x, y + 1]);
				}
			}
		}
		
		public List<MPos> CalculatePath(MPos start, MPos end, bool flying = false)
		{
			var path = new List<MPos>();

			// HACK: Clamp to borders to prevent crash in some special cases when target is at map border
			start = new MPos(Math.Clamp(start.X, 0, bounds.X - 1), Math.Clamp(start.Y, 0, bounds.Y - 1));
			end = new MPos(Math.Clamp(end.X, 0, bounds.X - 1), Math.Clamp(end.Y, 0, bounds.Y - 1));

			var startCell = cells[start.X, start.Y];
			var endCell = cells[end.X, end.Y];

			// A* search
			var queuedCells = new List<PathfinderCell>();
			queuedCells.Add(startCell);

			var visitedCells = new List<PathfinderCell>();

			var notFound = true;
			while (queuedCells.Count > 0)
			{
				var currentCell = queuedCells[0];
				queuedCells.RemoveAt(0);

				if (currentCell == endCell)
				{
					notFound = false;
					break;
				}

				foreach (var (cost, target) in currentCell.Connections)
				{
					if (visitedCells.Contains(target))
						continue;

					var newCost = currentCell.MovementCost + (flying ? 0 : cost);
					if (queuedCells.Contains(target) && newCost >= target.MovementCost)
						continue;

					target.Before = currentCell;
					target.MovementCost = newCost;
					queuedCells.Add(target);
				}

				visitedCells.Add(currentCell);
				queuedCells = queuedCells.OrderBy(c => c.MovementCost * movementCostFactor + c.HeuristicValueTo(end)).ToList();
			}

			if (notFound)
				return path;

			// Get the path
			var recursiveCell = endCell;
			while (recursiveCell != startCell)
			{
				path.Add(recursiveCell.Position);
				recursiveCell = recursiveCell.Before;
			}

			path.Reverse();

			return path;
		}

		public void RenderDebug()
		{
			CameraVisibility.GetClampedBounds(out var position, out var bounds);

			for (int x = position.X; x < position.X + bounds.X; x++)
			{
				for (int y = position.Y; y < position.Y + bounds.Y; y++)
				{
					var cell = cells[x, y];
					foreach (var (cost, target) in cell.Connections)
						ColorManager.DrawLine(cell.Position.ToCPos(), target.Position.ToCPos(), new Color(1f, cost / 5f, 0f, 0.2f));
				}
			}
		}

		class PathfinderCell
		{
			public readonly MPos Position;
			public float TerrainCost;

			public PathfinderCell Before;
			public float MovementCost;

			public readonly List<(float cost, PathfinderCell target)> Connections = new List<(float, PathfinderCell)>();

			public PathfinderCell(MPos pos)
			{
				Position = pos;
			}

			public float HeuristicValueTo(MPos pos)
			{
				return (Position - pos).SquaredDist;
			}
		}
	}
}
