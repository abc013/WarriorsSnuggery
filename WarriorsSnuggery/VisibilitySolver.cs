using System;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery
{
	public sealed class VisibilitySolver
	{
		static bool[,] visible = new bool[0, 0];
		static MPos bounds => map.Bounds;

		static MPos lastCameraPosition;
		static MPos lastCameraZoom;

		static Map map;
		static ShroudLayer shroud;

		public static void Reset()
		{
			visible = new bool[0, 0];
			map = null;
			shroud = null;
			lastCameraPosition = MPos.Zero;
			lastCameraZoom = MPos.Zero;
		}

		public static void SetBounds(Map map, ShroudLayer shroud)
		{
			VisibilitySolver.map = map;
			VisibilitySolver.shroud = shroud;
			visible = new bool[bounds.X, bounds.Y];
		}

		public static void ZoomUpdated()
		{
			var zoom = Camera.CurrentZoom;
			lastCameraZoom = Zoom(zoom);

			LookAtUpdated();
		}

		public static MPos Zoom(float zoom)
		{
			return new MPos((int)Math.Ceiling(zoom * WindowInfo.Ratio + 2), (int)Math.Ceiling(zoom + 2));
		}

		public static void LookAtUpdated()
		{
			var pos = Camera.LookAt;
			var zoom = Camera.CurrentZoom;
			lastCameraPosition = LookAt(pos, zoom);

			Array.Clear(visible, 0, visible.Length);
			ShroudUpdated();
		}

		public static MPos LookAt(CPos pos, float zoom)
		{
			var xPos = (int)Math.Floor(pos.X / 1024f - (zoom * WindowInfo.Ratio / 2));
			var yPos = (int)Math.Floor(pos.Y / 1024f - (zoom / 2));
			return new MPos(xPos, yPos);
		}

		public static MPos GetBounds(out MPos position)
		{
			position = lastCameraPosition;
			return lastCameraZoom;
		}

		public static void ShroudUpdated()
		{
			if (map == null)
				return;

			for (int x = lastCameraPosition.X; x < lastCameraPosition.X + lastCameraZoom.X; x++)
			{
				if (x >= 0 && x < bounds.X)
				{
					for (int y = lastCameraPosition.Y; y < lastCameraPosition.Y + lastCameraZoom.Y; y++)
					{
						if (y >= 0 && y < bounds.Y)
							visible[x, y] = checkShroud(x, y);
					}
				}
			}
		}

		static bool checkShroud(int x, int y)
		{
			if (shroud.RevealAll)
				return true;

			// As shroud has a doubled tilesize, we have to check 4 tiles.
			return shroud.ShroudRevealed(Objects.Actor.PlayerTeam, x * 2, y * 2)
				|| shroud.ShroudRevealed(Objects.Actor.PlayerTeam, x * 2 + 1, y * 2)
				|| shroud.ShroudRevealed(Objects.Actor.PlayerTeam, x * 2 + 1, y * 2 + 1)
				|| shroud.ShroudRevealed(Objects.Actor.PlayerTeam, x * 2, y * 2 + 1);
		}

		public static bool IsVisible(MPos position)
		{
			if (map == null)
				return false;

			if (shroud.RevealAll)
				return true;

			if (position.X < 0 || position.Y < 0 || position.X >= bounds.X || position.Y >= bounds.Y)
				return false;

			if (position.X < lastCameraPosition.X || position.Y < lastCameraPosition.Y || position.X >= lastCameraPosition.X + lastCameraZoom.X || position.Y >= lastCameraPosition.Y + lastCameraZoom.Y)
				return false;

			return visible[position.X, position.Y];
		}

		public static bool IsVisibleIgnoringBounds(MPos position)
		{
			if (map == null)
				return false;

			if (shroud.RevealAll)
				return true;

			if (position.X < lastCameraPosition.X || position.Y < lastCameraPosition.Y || position.X >= lastCameraPosition.X + lastCameraZoom.X || position.Y >= lastCameraPosition.Y + lastCameraZoom.Y)
				return false;

			if (position.X < 0)
				position = new MPos(0, position.Y);
			else if (position.X >= bounds.X)
				position = new MPos(bounds.X - 1, position.Y);

			if (position.Y < 0)
				position = new MPos(position.X, 0);
			else if (position.Y >= bounds.Y)
				position = new MPos(position.X, bounds.Y - 1);

			return visible[position.X, position.Y];
		}

		public static bool IsVisible(CPos position)
		{
			if (position.X < map.TopLeftCorner.X || position.Y < map.TopLeftCorner.Y)
				return false;

			if (position.X >= map.BottomRightCorner.X || position.Y >= map.BottomRightCorner.Y)
				return false;

			return IsVisibleIgnoringBounds(position.ToMPos());
		}

		public static bool IsVisible(CPos position, MPos scale)
		{
			return IsVisible(new CPos(position.X + scale.X, position.Y + scale.Y, position.Z))
				|| IsVisible(new CPos(position.X - scale.X, position.Y + scale.Y, position.Z))
				|| IsVisible(new CPos(position.X - scale.X, position.Y - scale.Y, position.Z))
				|| IsVisible(new CPos(position.X + scale.X, position.Y - scale.Y, position.Z));
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
