using System;

namespace WarriorsSnuggery.Graphics
{
	public sealed class CameraVisibility
	{
		static MPos mapBounds;

		static MPos lastCameraPosition;
		static MPos lastCameraZoom;

		public static void Reset()
		{
			mapBounds = MPos.Zero;
			lastCameraPosition = MPos.Zero;
			lastCameraZoom = MPos.Zero;
		}

		public static void SetBounds(MPos bounds)
		{
			mapBounds = bounds;
		}

		internal static void ZoomUpdated()
		{
			var zoom = Camera.CurrentZoom;
			lastCameraZoom = new MPos((int)Math.Ceiling(zoom * WindowInfo.Ratio), (int)Math.Ceiling(zoom));

			LookAtUpdated();
		}

		internal static void LookAtUpdated()
		{
			var pos = Camera.LookAt;
			var zoom = Camera.CurrentZoom;

			var xPos = (int)Math.Floor(pos.X / (float)Constants.TileSize - (zoom * WindowInfo.Ratio / 2));
			var yPos = (int)Math.Floor(pos.Y / (float)Constants.TileSize - (zoom / 2));
			lastCameraPosition = new MPos(xPos, yPos);
		}

		public static void GetClampedBounds(out MPos position, out MPos bounds)
		{
			var offset = Settings.VisibilityMargin / Constants.TileSize;
			position = new MPos(Math.Clamp(lastCameraPosition.X - offset, 0, mapBounds.X), Math.Clamp(lastCameraPosition.Y - offset, 0, mapBounds.Y));
			bounds = new MPos(Math.Clamp(lastCameraZoom.X + lastCameraPosition.X + offset, 0, mapBounds.X) - position.X, Math.Clamp(lastCameraZoom.Y + lastCameraPosition.Y + offset, 0, mapBounds.Y) - position.Y);
		}
	}
}
