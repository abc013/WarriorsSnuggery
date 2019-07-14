/*
 * User: Andreas
 * Date: 02.08.2018
 * Time: 12:24
 */
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
		}

		public void Remove(MPos pos)
		{
			Walls[pos.X, pos.Y]?.Dispose();
			Walls[pos.X, pos.Y] = null;
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
