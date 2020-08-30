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

			var pos1 = new MPos((int)Math.Clamp(Math.Floor(topleft.X / 1024f), 0, mapBounds.X), (int)Math.Clamp(Math.Floor(topleft.Y / 1024f), 0, mapBounds.Y));
			var pos2 = new MPos((int)Math.Clamp(Math.Ceiling(botright.X / 1024f), 0, mapBounds.X), (int)Math.Clamp(Math.Ceiling(botright.Y / 1024f), 0, mapBounds.Y));

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
			if (Walls[pos.X, pos.Y] == null)
				return;

			var toRemove = Walls[pos.X, pos.Y];
			toRemove.Dispose();
			WallList.Remove(toRemove);
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

					|2
					|
			1		|		3
			--------o--------
					|
					|
					|
			--------o--------
			6		|		8
					|
				   7|

					|1		|6
					|		|
			4		|		|	    5
			--------o-------o--------
					|	    |
					|		|
				   3|		|8
			*/

			if (pos.X % 2 != 0)
			{
				// Horizontal

				if (left && Walls[pos.X - 2, pos.Y] != null && !Walls[pos.X - 2, pos.Y].Type.IgnoreForNearby)
				{
					if (!ignoresNearby)
						Walls[pos.X - 2, pos.Y].SetNeighborState(0b00010000, added);
					s |= 0b00001000;
				}

				if (top && Walls[pos.X - 1, pos.Y - 1] != null && !Walls[pos.X - 1, pos.Y - 1].Type.IgnoreForNearby)
				{
					if (!ignoresNearby)
						Walls[pos.X - 1, pos.Y - 1].SetNeighborState(0b10000000, added);
					s |= 0b00000001;
				}

				if (Walls[pos.X - 1, pos.Y] != null && !Walls[pos.X - 1, pos.Y].Type.IgnoreForNearby)
				{
					if (!ignoresNearby)
						Walls[pos.X - 1, pos.Y].SetNeighborState(0b00100000, added);
					s |= 0b00100000;
				}

				if (right)
				{
					if (top && Walls[pos.X + 1, pos.Y - 1] != null && !Walls[pos.X + 1, pos.Y - 1].Type.IgnoreForNearby)
					{
						if (!ignoresNearby)
							Walls[pos.X + 1, pos.Y - 1].SetNeighborState(0b00000100, added);
						s |= 0b00000100;
					}

					if (Walls[pos.X + 1, pos.Y] != null && !Walls[pos.X + 1, pos.Y].Type.IgnoreForNearby)
					{
						if (!ignoresNearby)
							Walls[pos.X + 1, pos.Y].SetNeighborState(0b00000001, added);
						s |= 0b10000000;
					}

					if (Walls[pos.X + 2, pos.Y] != null && !Walls[pos.X + 2, pos.Y].Type.IgnoreForNearby)
					{
						if (!ignoresNearby)
							Walls[pos.X + 2, pos.Y].SetNeighborState(0b00001000, added);
						s |= 0b00010000;
					}
				}
			}
			else
			{
				// Vertical

				if (left && Walls[pos.X - 1, pos.Y] != null && !Walls[pos.X - 1, pos.Y].Type.IgnoreForNearby)
				{
					if (!ignoresNearby)
						Walls[pos.X - 1, pos.Y].SetNeighborState(0b10000000, added);
					s |= 0b00000001;
				}

				if (top && Walls[pos.X, pos.Y - 1] != null && !Walls[pos.X, pos.Y - 1].Type.IgnoreForNearby)
				{
					if (!ignoresNearby)
						Walls[pos.X, pos.Y - 1].SetNeighborState(0b01000000, added);
					s |= 0b00000010;
				}

				if (Walls[pos.X + 1, pos.Y] != null && !Walls[pos.X + 1, pos.Y].Type.IgnoreForNearby)
				{
					if (!ignoresNearby)
						Walls[pos.X + 1, pos.Y]?.SetNeighborState(0b00100000, added);
					s |= 0b00100000;
				}

				if (bottom)
				{
					if (Walls[pos.X, pos.Y + 1] != null && !Walls[pos.X, pos.Y + 1].Type.IgnoreForNearby)
					{
						if (!ignoresNearby)
							Walls[pos.X, pos.Y + 1]?.SetNeighborState(0b00000010, added);
						s |= 0b01000000;
					}

					if (Walls[pos.X + 1, pos.Y + 1] != null && !Walls[pos.X + 1, pos.Y + 1].Type.IgnoreForNearby)
					{
						if (!ignoresNearby)
							Walls[pos.X + 1, pos.Y + 1]?.SetNeighborState(0b00000001, added);
						s |= 0b00010000;
					}

					if (left && Walls[pos.X - 1, pos.Y + 1] != null && !Walls[pos.X - 1, pos.Y + 1].Type.IgnoreForNearby)
					{
						if (!ignoresNearby)
							Walls[pos.X - 1, pos.Y + 1]?.SetNeighborState(0b00010000, added);
						s |= 0b00001000;
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
