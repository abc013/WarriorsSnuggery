using System;
using System.Linq;
using System.Collections.Generic;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery
{
	public sealed class WallLayer : IRenderable
	{
		public readonly List<Wall> WallList = new List<Wall>();
		public readonly List<Wall> VisibleWalls = new List<Wall>();
		public Wall[,] Walls { get; private set; }
		public MPos Bounds { get; private set; }
		MPos mapBounds;

		public WallLayer(MPos bounds)
		{
			mapBounds = bounds;
			Bounds = new MPos((bounds.X + 1) * 2 + 1, (bounds.Y + 1) + 1);
			Walls = new Wall[Bounds.X, Bounds.Y];
		}

		public List<Wall> GetRange(CPos position, int radius)
		{
			var topleft = position - new CPos(radius, radius, 0) - Map.Offset;
			var botright = position + new CPos(radius, radius, 0) - Map.Offset;

			var pos1 = new MPos((int)Math.Clamp(Math.Floor(topleft.X / 1024f), 0, mapBounds.X + 1), (int)Math.Clamp(Math.Floor(topleft.Y / 1024f), 0, mapBounds.Y + 1));
			var pos2 = new MPos((int)Math.Clamp(Math.Ceiling(botright.X / 1024f), 0, mapBounds.X + 1), (int)Math.Clamp(Math.Ceiling(botright.Y / 1024f), 0, mapBounds.Y + 1));

			return WallList.Where(w => w.TerrainPosition.X >= pos1.X && w.TerrainPosition.X < pos2.X && w.TerrainPosition.Y >= pos1.Y && w.TerrainPosition.Y < pos2.Y).ToList();
		}

		public void Set(Wall wall)
		{
			Remove(wall.LayerPosition);
			Walls[wall.LayerPosition.X, wall.LayerPosition.Y] = wall;
			notifyNeighbors(wall.LayerPosition, true, wall.Type.IgnoreForNearby);
			WallList.Add(wall);
			if (wall.CheckVisibility())
				VisibleWalls.Add(wall);
		}

		public void Remove(MPos pos)
		{
			var wall = Walls[pos.X, pos.Y];

			if (wall == null)
				return;

			var toRemove = wall;
			toRemove.Dispose();
			WallList.Remove(toRemove);
			VisibleWalls.Remove(toRemove);
			Walls[pos.X, pos.Y] = null;
			notifyNeighbors(pos, false, false);
		}

		void notifyNeighbors(MPos pos, bool added, bool ignoresNearby)
		{
			byte s = 0;

			bool left = pos.X >= 2;
			bool right = pos.X < Bounds.X - 3;
			bool top = pos.Y > 0;
			bool bottom = pos.Y < Bounds.Y - 2;

			/*

			HORIZONTAL
					|0		|3
					|		|
			1		|		|	    4
			--------o-------o--------
					|	    |
					|		|
				   2|		|5

			VERTICAL
					|1
					|
			0		|		2
			--------o--------
					|
					|
					|
			--------o--------
			3		|		5
					|
				   4|

			BYTE 012 345 67
			     000 000 --
			*/

			Wall wall;
			if (pos.X % 2 != 0)
			{
				// Horizontal
				if (top)
				{
					wall = Walls[pos.X - 1, pos.Y - 1];
					if (wall != null && !wall.Type.IgnoreForNearby)
					{
						if (!ignoresNearby)
							wall.SetNeighborState(0b00000100, added);
						s |= 0b10000000;
					}
				}

				if (left)
				{
					wall = Walls[pos.X - 2, pos.Y];
					if (wall != null && !wall.Type.IgnoreForNearby)
					{
						if (!ignoresNearby)
							wall.SetNeighborState(0b00001000, added);
						s |= 0b01000000;
					}
				}

				wall = Walls[pos.X - 1, pos.Y];
				if (wall != null && !wall.Type.IgnoreForNearby)
				{
					if (!ignoresNearby)
						wall.SetNeighborState(0b00100000, added);
					s |= 0b00100000;
				}

				if (right)
				{
					if (top)
					{
						wall = Walls[pos.X + 1, pos.Y - 1];
						if (wall != null && !wall.Type.IgnoreForNearby)
						{
							if (!ignoresNearby)
								wall.SetNeighborState(0b00010000, added);
							s |= 0b00010000;
						}
					}

					wall = Walls[pos.X + 2, pos.Y];
					if (wall != null && !wall.Type.IgnoreForNearby)
					{
						if (!ignoresNearby)
							wall.SetNeighborState(0b01000000, added);
						s |= 0b00001000;
					}

					wall = Walls[pos.X + 1, pos.Y];
					if (wall != null && !wall.Type.IgnoreForNearby)
					{
						if (!ignoresNearby)
							wall.SetNeighborState(0b10000000, added);
						s |= 0b00000100;
					}
				}
			}
			else
			{
				// Vertical
				if (left)
				{
					wall = Walls[pos.X - 1, pos.Y];
					if (wall != null && !wall.Type.IgnoreForNearby)
					{
						if (!ignoresNearby)
							wall.SetNeighborState(0b00000100, added);
						s |= 0b10000000;
					}
				}

				if (top)
				{
					wall = Walls[pos.X, pos.Y - 1];
					if (wall != null && !wall.Type.IgnoreForNearby)
					{
						if (!ignoresNearby)
							wall.SetNeighborState(0b00001000, added);
						s |= 0b01000000;
					}
				}

				wall = Walls[pos.X + 1, pos.Y];
				if (wall != null && !wall.Type.IgnoreForNearby)
				{
					if (!ignoresNearby)
						wall.SetNeighborState(0b00100000, added);
					s |= 0b00100000;
				}

				if (bottom)
				{
					if (left)
					{
						wall = Walls[pos.X - 1, pos.Y + 1];
						if (wall != null && !wall.Type.IgnoreForNearby)
						{
							if (!ignoresNearby)
								wall.SetNeighborState(0b00010000, added);
							s |= 0b00010000;
						}
					}

					wall = Walls[pos.X, pos.Y + 1];
					if (wall != null && !wall.Type.IgnoreForNearby)
					{
						if (!ignoresNearby)
							wall.SetNeighborState(0b01000000, added);
						s |= 0b00001000;
					}

					wall = Walls[pos.X + 1, pos.Y + 1];
					if (wall != null && !wall.Type.IgnoreForNearby)
					{
						if (!ignoresNearby)
							wall.SetNeighborState(0b10000000, added);
						s |= 0b00000100;
					}
				}
			}

			if (added)
				Walls[pos.X, pos.Y].SetNeighborState(s, true);
		}

		public void CheckVisibility()
		{
			foreach (var w in WallList)
				w.CheckVisibility();
			VisibleWalls.Clear();
			VisibleWalls.AddRange(WallList);
		}

		public void CheckVisibility(MPos bottomleft, MPos topright)
		{
			VisibleWalls.Clear();

			for (int x = bottomleft.X; x < topright.X * 2 + 1; x++)
			{
				for (int y = bottomleft.Y; y < topright.Y + 1; y++)
				{
					var wall = Walls[x, y];
					if (wall != null && wall.CheckVisibility())
						VisibleWalls.Add(wall);
				}
			}
		}

		public void Render()
		{
			foreach (var wall in WallList)
				wall.Render();
		}

		public void Clear()
		{
			Walls = new Wall[0, 0];
			mapBounds = MPos.Zero;
			Bounds = MPos.Zero;

			foreach (var wall in WallList)
				wall.Dispose();
			WallList.Clear();
		}
	}
}
