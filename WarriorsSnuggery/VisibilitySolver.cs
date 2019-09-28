using System;

namespace WarriorsSnuggery
{
	public sealed class VisibilitySolver
	{
		static bool[,] visible = new bool[0, 0];
		static MPos size = MPos.Zero;

		public static MPos lastCameraPosition;
		public static MPos lastCameraZoom;

		static ShroudLayer shroud;

		public static void Reset()
		{
			size = MPos.Zero;
			visible = new bool[0, 0];
			shroud = null;
			lastCameraPosition = MPos.Zero;
			lastCameraZoom = MPos.Zero;
		}

		public static void SetMapDimensions(MPos size, ShroudLayer shroud)
		{
			Reset();
			VisibilitySolver.size = size;
			VisibilitySolver.shroud = shroud;
			visible = new bool[size.X, size.Y];
		}

		public static void ZoomUpdated()
		{
			var zoom = Camera.CurrentZoom;
			lastCameraZoom = new MPos((int)Math.Ceiling(zoom * WindowInfo.Ratio + 2), (int)Math.Ceiling(zoom + 2));

			LookAtUpdated();
		}

		public static void LookAtUpdated()
		{
			var pos = Camera.LookAt;
			var zoom = Camera.CurrentZoom;

			var xPos = (int)Math.Floor(pos.X / 1024f - (zoom * WindowInfo.Ratio / 2));
			var yPos = (int)Math.Floor(pos.Y / 1024f - (zoom / 2));
			lastCameraPosition = new MPos(xPos, yPos);

			cameraUpdated();
		}

		static void cameraUpdated()
		{
			clear();
			// Is the same as we would use here
			ShroudUpdated();
		}

		public static void ShroudUpdated()
		{
			for (int x = lastCameraPosition.X; x < lastCameraPosition.X + lastCameraZoom.X; x++)
			{
				if (x >= 0 && x < size.X)
				{
					for (int y = lastCameraPosition.Y; y < lastCameraPosition.Y + lastCameraZoom.Y; y++)
					{
						if (y >= 0 && y < size.Y)
						{
							visible[x, y] = checkShroud(x, y);
						}
					}
				}
			}
		}

		static bool checkShroud(int x, int y)
		{
			if (shroud.AllRevealed)
				return true;

			// As shroud has a doubled tilesize, we have to check 4 tiles.
			var con1 = shroud.ShroudRevealed(Objects.Actor.PlayerTeam, x * 2, y * 2);
			var con2 = shroud.ShroudRevealed(Objects.Actor.PlayerTeam, x * 2 + 1, y * 2);
			var con3 = shroud.ShroudRevealed(Objects.Actor.PlayerTeam, x * 2 + 1, y * 2 + 1);
			var con4 = shroud.ShroudRevealed(Objects.Actor.PlayerTeam, x * 2, y * 2 + 1);
			return con1 || con2 || con3 || con4;
		}

		public static bool IsVisible(WPos position)
		{
			if (position.X < 0 || position.Y < 0 || position.X >= size.X || position.Y >= size.Y)
				return false;

			return visible[position.X, position.Y];
		}

		public static bool IsVisibleIgnoringBounds(WPos position)
		{
			if (size == MPos.Zero)
				return false;

			if (position.X < 0)
			{
				position = new WPos(0, position.Y, 0);
			}
			else if (position.X >= size.X)
			{
				position = new WPos(size.X - 1, position.Y, 0);
			}

			if (position.Y < 0)
			{
				position = new WPos(position.X, 0, 0);
			}
			else if (position.Y >= size.Y)
			{
				position = new WPos(position.X, size.Y - 1, 0);
			}

			return visible[position.X, position.Y];
		}

		public static bool IsVisible(CPos position)
		{
			if (position.X < 0 || position.Y < 0)
				return false;

			if (position.X >= size.X * 1024 - 1024 || position.Y >= size.Y * 1024 - 1024)
				return false;

			var mPos = position.ToMPos();

			return visible[mPos.X, mPos.Y];
		}

		public static bool IsVisible(CPos position, MPos scale)
		{
			// Only check for corners since this should solve most of the cases.
			return IsVisible(new CPos(position.X + scale.X, position.Y + scale.Y, position.Z))
				|| IsVisible(new CPos(position.X - scale.X, position.Y + scale.Y, position.Z))
				|| IsVisible(new CPos(position.X - scale.X, position.Y - scale.Y, position.Z))
				|| IsVisible(new CPos(position.X + scale.X, position.Y - scale.Y, position.Z));
		}

		static void clear()
		{
			visible = new bool[size.X, size.Y];
		}

		public static int TilesVisible()
		{
			var count = 0;
			foreach (var value in visible)
			{
				if (value)
					count++;
			}

			return count;
		}
	}
}
