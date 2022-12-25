using OpenTK.Mathematics;

namespace WarriorsSnuggery.Graphics
{
	public static class Camera
	{
		public static CPos LookAt { get; private set; }

		public static MPos LookBounds { get; private set; }

		public static bool Locked;
		public static bool LockedToPlayer = true;
		public static readonly CPos CameraPlayerOffset = new CPos(0, 512, 0);

		public const float UIZoom = 16f;
		public const float DefaultZoom = 12f;
		public const float MaxZoom = 24f;
		public static float CurrentZoom = DefaultZoom;

		public static Matrix4 Matrix = Matrix4.Zero;

		static Matrix4 projection = Matrix4.Zero;
		static Matrix4 view = Matrix4.Zero;

		static CPos bounds = CPos.Zero;

		public static void SetBounds(MPos bounds)
		{
			Camera.bounds = (bounds - new MPos(1, 1)).ToCPos();
		}

		public static bool IsVisible(CPos pos, float scaleX, float scaleY)
		{
			var halfzoomX = LookBounds.X / 2 + 512;
			var halfzoomY = LookBounds.Y / 2 + 512;

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

			if (bounds != CPos.Zero)
			{
				if (LookAt.X < -512)
					LookAt = new CPos(-512, LookAt.Y, 0);

				if (LookAt.Y < -512)
					LookAt = new CPos(LookAt.X, -512, 0);

				if (LookAt.X > bounds.X + 512)
					LookAt = new CPos(bounds.X + 512, LookAt.Y, 0);

				if (LookAt.Y > bounds.Y + 512)
					LookAt = new CPos(LookAt.X, bounds.Y + 512, 0);
			}

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

		public static CPos GetWindowCoordinates(CPos gamePos)
		{
			var diff = gamePos - LookAt;

			var x = diff.X / CurrentZoom * UIZoom;
			var y = diff.Y / CurrentZoom * UIZoom;

			return new CPos((int)x, (int)y, 0);
		}

		public static CPos GetGameCoordinates(CPos windowPos)
		{
			var x = windowPos.X / UIZoom * CurrentZoom;
			var y = windowPos.Y / UIZoom * CurrentZoom;

			return new CPos((int)x, (int)y, 0) + LookAt;
		}
	}
}
