using System;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery
{
	public sealed class WallLayer : IRenderable, IDisposable
	{
		public Wall[,] Walls { get; private set; }
		public MPos Size { get; private set; }

		public WallLayer()
		{
			Walls = new Wall[0, 0];
			Size = MPos.Zero;
		}

		public void SetMapSize(MPos size)
		{
			Dispose();
			Size = new MPos((size.X + 1) * 2 + 1, (size.Y + 1) + 1);
			Walls = new Wall[Size.X, Size.Y];
		}

		public void Set(Wall wall)
		{
			Walls[wall.LayerPosition.X, wall.LayerPosition.Y] = wall;
			notifyNeighbors(wall.LayerPosition, true);
		}

		public void Remove(MPos pos)
		{
			if (Walls[pos.X, pos.Y] == null)
				return;

			Walls[pos.X, pos.Y].Dispose();
			Walls[pos.X, pos.Y] = null;
			notifyNeighbors(pos, false);
		}

		void notifyNeighbors(MPos pos, bool added)
		{
			bool s1 = false;
			bool s2 = false;

			bool left = pos.X >= 2;
			bool right = pos.X < Size.X - 3;
			bool top = pos.Y > 0;
			bool bottom = pos.Y < Size.Y - 2;

			if (left)
			{
				Walls[pos.X - 1 - (pos.X % 2), pos.Y]?.SetNeighborState(0b10000000, false, added);
				s1 |= Walls[pos.X - 1 - (pos.X % 2), pos.Y] != null;
			}

			if (top)
			{
				Walls[pos.X - (pos.X % 2), pos.Y - 1]?.SetNeighborState(0b01000000, false, added);
				s1 |= Walls[pos.X - (pos.X % 2), pos.Y - 1] != null;
			}

			if (pos.X % 2 != 0)
			{
				Walls[pos.X - 1, pos.Y]?.SetNeighborState(0b00001000, true, added);
				s1 |= Walls[pos.X - 1, pos.Y] != null;

				if (right)
				{
					if (top)
					{
						Walls[pos.X + 1, pos.Y - 1]?.SetNeighborState(0b00100000, false, added);
						s2 |= Walls[pos.X + 1, pos.Y - 1] != null;
					}

					Walls[pos.X + 1, pos.Y]?.SetNeighborState(0b00000100, true, added);
					s2 |= Walls[pos.X + 1, pos.Y] != null;

					Walls[pos.X + 2, pos.Y]?.SetNeighborState(0b00000010, true, added);
					s2 |= Walls[pos.X + 2, pos.Y] != null;
				}
			}
			else
			{
				Walls[pos.X + 1, pos.Y]?.SetNeighborState(0b00001000, true, added);
				s1 |= Walls[pos.X + 1, pos.Y] != null;

				if (bottom)
				{
					Walls[pos.X, pos.Y + 1]?.SetNeighborState(0b000000100, true, added);
					s2 |= Walls[pos.X, pos.Y + 1] != null;

					Walls[pos.X + 1, pos.Y + 1]?.SetNeighborState(0b00000010, true, added);
					s2 |= Walls[pos.X + 1, pos.Y + 1] != null;

					if (left)
					{
						Walls[pos.X - 1, pos.Y + 1]?.SetNeighborState(0b00100000, false, added);
						s2 |= Walls[pos.X - 1, pos.Y + 1] != null;
					}
				}
			}

			if (added)
			{
				Walls[pos.X, pos.Y].SetNeighborState((byte)(s1 ? 0b00001000 : 0b00000000), true, s1);
				Walls[pos.X, pos.Y].SetNeighborState((byte)(s2 ? 0b10000000 : 0b00000000), false, s2);
			}
		}

		public void Render()
		{
			foreach (var wall in Walls)
				wall?.Render();
		}

		public void Dispose()
		{
			foreach (var wall in Walls)
				wall?.Dispose();
		}
	}
}
