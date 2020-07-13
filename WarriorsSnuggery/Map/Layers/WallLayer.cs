using System;
using System.Collections.Generic;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery
{
	public sealed class WallLayer : IRenderable, IDisposable
	{
		public readonly List<Wall> WallList = new List<Wall>();
		public Wall[,] Walls { get; private set; }
		public MPos Bounds { get; private set; }

		public WallLayer()
		{
			Walls = new Wall[0, 0];
		}

		public void SetBounds(MPos bounds)
		{
			Dispose();
			Bounds = new MPos((bounds.X + 1) * 2 + 1, (bounds.Y + 1) + 1);
			Walls = new Wall[Bounds.X, Bounds.Y];
		}

		public void Set(Wall wall)
		{
			Remove(wall.LayerPosition);
			Walls[wall.LayerPosition.X, wall.LayerPosition.Y] = wall;
			notifyNeighbors(wall.LayerPosition, true, wall.Type.IgnoreForNearby);
			WallList.Add(wall);
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

		public void Render()
		{
			foreach (var wall in WallList)
				wall.Render();
		}

		public void Dispose()
		{
			Walls = new Wall[0, 0];
			Bounds = MPos.Zero;
			foreach (var wall in WallList)
				wall.Dispose();
			WallList.Clear();
		}
	}
}
