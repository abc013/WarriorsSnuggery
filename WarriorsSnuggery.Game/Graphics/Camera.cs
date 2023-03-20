using OpenTK.Mathematics;
using WarriorsSnuggery.Maps;

namespace WarriorsSnuggery.Graphics
{
	public static class UICamera
	{
		public const float Zoom = 16f;

		public static Matrix4 Matrix = Matrix4.Zero;

		public static void Update()
		{
			Matrix = Matrix4.CreateScale(2 / (Zoom * WindowInfo.Ratio), 2 / Zoom, 1f);
		}
	}

	public static class Camera
	{
		public static CPos LookAt { get; private set; }
		public static MPos LookBounds { get; private set; }

		public static bool Locked;

		public const float DefaultZoom = 12f;
		public const float MaxZoom = DefaultZoom * 2;
		public static float CurrentZoom { get; private set; } = DefaultZoom;

		public static Matrix4 Matrix = Matrix4.Zero;

		static Matrix4 projection = Matrix4.Zero;
		static Matrix4 view = Matrix4.Zero;

		static CPos bounds = CPos.Zero;

		public static void SetBounds(MPos bounds)
		{
			Camera.bounds = bounds.ToCPos();
			CameraVisibility.SetBounds(bounds);
		}

		public static bool IsVisible(CPos pos, float scaleX, float scaleY)
		{
			const int margin = Constants.TileSize / 2;

			var halfzoomX = LookBounds.X / 2 + margin;
			var halfzoomY = LookBounds.Y / 2 + margin;

			if (pos.X + scaleX < LookAt.X - halfzoomX)
				return false;

			if (pos.X - scaleX > LookAt.X + halfzoomX)
				return false;

			if (pos.Y + scaleY < LookAt.Y - halfzoomY)
				return false;

			if (pos.Y - scaleY > LookAt.Y + halfzoomY)
				return false;

			return true;
		}

		public static void Reset()
		{
			CameraVisibility.Reset();
			LookAt = CPos.Zero;
			Locked = false;
			CurrentZoom = DefaultZoom;

			Update();
		}

		public static void Update()
		{
			calculatePosition();
			calculateScale();
			updateView();
		}

		public static void Zoom(float add, bool ignoreLock = false)
		{
			if (!ignoreLock && Locked || add == 0)
				return;

			CurrentZoom += add;
			if (CurrentZoom < DefaultZoom)
				CurrentZoom = DefaultZoom;
			else if (CurrentZoom > MaxZoom)
				CurrentZoom = MaxZoom;

			calculateScale();
			updateView();
		}

		public static void Move(CPos add, bool ignoreLock = false)
		{
			if (!ignoreLock && Locked || add == CPos.Zero)
				return;

			LookAt = new CPos(LookAt.X + (int)(Settings.ScrollSpeed * 20 * add.X), LookAt.Y + (int)(Settings.ScrollSpeed * 20 * add.Y), 0);

			if (LookAt.X < Map.Offset.X)
				LookAt = new CPos(Map.Offset.X, LookAt.Y, 0);

			if (LookAt.Y < Map.Offset.Y)
				LookAt = new CPos(LookAt.X, Map.Offset.Y, 0);

			if (LookAt.X > bounds.X + Map.Offset.X)
				LookAt = new CPos(bounds.X + Map.Offset.X, LookAt.Y, 0);

			if (LookAt.Y > bounds.Y + Map.Offset.Y)
				LookAt = new CPos(LookAt.X, bounds.Y + Map.Offset.Y, 0);

			calculatePosition();
			updateView();
		}

		public static void Position(CPos pos, bool ignoreLock = false, bool tinyMove = false)
		{
			if (!ignoreLock && Locked || LookAt == pos)
				return;

			LookAt = pos;

			calculatePosition();
			updateView();
		}

		static void calculatePosition()
		{
			var look = (LookAt + Screenshaker.RandomShake).ToVector();
			view = Matrix4.CreateTranslation(-look.X, -look.Y, 0);

			CameraVisibility.LookAtUpdated();
		}

		static void calculateScale()
		{
			LookBounds = new MPos((int)(CurrentZoom * WindowInfo.Ratio * Constants.TileSize), (int)(CurrentZoom * Constants.TileSize));
			// cast to [-1;1] | we use 0f to stop things glitching out of sight
			projection = Matrix4.CreateScale(2 / CurrentZoom / WindowInfo.Ratio, 2 / CurrentZoom, 0f);

			CameraVisibility.ZoomUpdated();
		}

		static void updateView()
		{
			Matrix = view * projection;

			MouseInput.RecalculateMousePosition();
		}
	}
}
