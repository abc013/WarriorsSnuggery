using System;

namespace WarriorsSnuggery
{
	public sealed class VisibilitySolver
	{
		static bool[,] visible = new bool[0,0];
		static MPos size = MPos.Zero;

		public static void Reset()
		{
			size = MPos.Zero;
			visible = new bool[0, 0];
		}

		public static void SetMapDimensions(MPos size)
		{
			Dispose();
			VisibilitySolver.size = size;
			visible = new bool[size.X, size.Y];
		}

		public static void CameraUpdated()
		{
			var pos = Camera.LookAt;
			var zoomX = Camera.CurrentZoom * WindowInfo.Ratio;
			var zoomY = Camera.CurrentZoom;

			clear();

			var xPos = (int)Math.Floor(pos.X / 1024f - (zoomX / 2));
			var yPos = (int)Math.Floor(pos.Y / 1024f - (zoomY / 2));

			for (int x = xPos; x < xPos + (int)Math.Ceiling(zoomX) + 1; x++)
			{
				if (x >= 0 && x < size.X)
				{
					for (int y = yPos; y < yPos + (int)Math.Ceiling(zoomY + WindowInfo.Ratio); y++)
					{
						if (y >= 0 && y < size.Y)
						{
							visible[x, y] = true;
						}
					}
				}
			}
		}

		public static void ShroudUpdated(ShroudLayer layer)
		{

		}

		public static bool IsVisible(WPos position)
		{
			if (position.X < 0 || position.Y < 0 || position.X >= size.X || position.Y >= size.Y)
				return false;

			return visible[position.X, position.Y];
		}

		public static bool IsVisible(CPos position)
		{
			if (position.X < 0 || position.Y < 0 || position.X >= size.X * 1024 || position.Y >= size.Y * 1024)
				return false;

			var mPos = position.ToMPos();

			return visible[mPos.X, mPos.Y];
		}

		public static bool IsVisible(CPos position, MPos scale)
		{
			// Only check for corners since this should solve most of the cases.
			return IsVisible(new CPos(position.X + scale.X, position.Y + scale.Y, position.Z))
				&& IsVisible(new CPos(position.X - scale.X, position.Y + scale.Y, position.Z))
				&& IsVisible(new CPos(position.X - scale.X, position.Y - scale.Y, position.Z))
				&& IsVisible(new CPos(position.X + scale.X, position.Y - scale.Y, position.Z));
		}

		static void clear()
		{
			visible = new bool[size.X, size.Y];
		}

		public static int FieldsVisible()
		{
			var count = 0;
			foreach (var value in visible)
			{
				if (value)
					count++;
			}

			return count;
		}

		public static void Dispose()
		{
			visible = new bool[0, 0];
		}
	}
}
