using System;
using WarriorsSnuggery.Maps;
using WarriorsSnuggery.Maps.Layers;
using WarriorsSnuggery.Objects.Actors;

namespace WarriorsSnuggery.Graphics
{
	public sealed class CameraVisibility
	{
		static bool[,] visible = new bool[0, 0];
		static MPos mapBounds => map.Bounds;

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
			CameraVisibility.map = map;
			CameraVisibility.shroud = shroud;
			visible = new bool[mapBounds.X, mapBounds.Y];
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

		public static void GetClampedBounds(out MPos position, out MPos bounds)
		{
			position = new MPos(Math.Clamp(lastCameraPosition.X, 0, mapBounds.X), Math.Clamp(lastCameraPosition.Y, 0, mapBounds.Y));
			bounds = new MPos(Math.Clamp(lastCameraZoom.X + lastCameraPosition.X, 0, mapBounds.X) - position.X, Math.Clamp(lastCameraZoom.Y + lastCameraPosition.Y, 0, mapBounds.Y) - position.Y);
		}

		public static void ShroudUpdated()
		{
			if (map == null)
				return;

			GetClampedBounds(out var position, out var bounds);

			for (int x = position.X; x < position.X + bounds.X; x++)
			{
				for (int y = position.Y; y < position.Y + bounds.Y; y++)
					visible[x, y] = checkShroud(x, y);
			}
		}

		public static void ShroudRevealed(int x, int y)
		{
			if (map == null)
				return;

			visible[x, y] = true;
		}

		static bool checkShroud(int x, int y)
		{
			if (shroud.RevealAll)
				return true;

			// As shroud has a doubled tilesize, we have to check 4 tiles.
			return shroud.ShroudRevealed(Actor.PlayerTeam, x * 2, y * 2)
				|| shroud.ShroudRevealed(Actor.PlayerTeam, x * 2 + 1, y * 2)
				|| shroud.ShroudRevealed(Actor.PlayerTeam, x * 2 + 1, y * 2 + 1)
				|| shroud.ShroudRevealed(Actor.PlayerTeam, x * 2, y * 2 + 1);
		}

		public static bool IsVisible(MPos position)
		{
			if (map == null)
				return false;

			if (shroud.RevealAll)
				return true;

			if (!position.InRange(lastCameraPosition, lastCameraPosition + lastCameraZoom))
				return false;

			return visible[position.X, position.Y];
		}

		public static bool IsVisibleIgnoringBounds(MPos position)
		{
			if (map == null)
				return false;

			if (shroud.RevealAll)
				return true;

			if (!position.InRange(lastCameraPosition, lastCameraPosition + lastCameraZoom))
				return false;

			if (position.X < 0)
				position = new MPos(0, position.Y);
			else if (position.X >= mapBounds.X)
				position = new MPos(mapBounds.X - 1, position.Y);

			if (position.Y < 0)
				position = new MPos(position.X, 0);
			else if (position.Y >= mapBounds.Y)
				position = new MPos(position.X, mapBounds.Y - 1);

			return visible[position.X, position.Y];
		}

		public static bool IsVisible(CPos position)
		{
			if (position.X < map.TopLeftCorner.X || position.Y < map.TopLeftCorner.Y)
				return false;

			if (position.X >= map.BottomRightCorner.X || position.Y >= map.BottomRightCorner.Y)
				return false;

			return IsVisibleIgnoringBounds(position.ToMPosConsideringTerrainOffset());
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
