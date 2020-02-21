using OpenTK;

namespace WarriorsSnuggery
{
	public static class Camera
	{
		public static readonly CPos CamPlayerOffset = new CPos(0, 2048, 0);

		public static CPos LookAt = CPos.Zero;

		public static bool Locked;
		public static bool LockedToPlayer = true;

		public const float DefaultZoom = 16f;
		public static float CurrentZoom = DefaultZoom;

		public static Matrix4 Matrix = Matrix4.Zero;

		static Matrix4 projection = Matrix4.Zero;
		static Matrix4 view = Matrix4.Zero;

		static CPos bounds = CPos.Zero;

		public static void SetBounds(MPos bounds)
		{
			Camera.bounds = bounds.ToCPos() + new CPos(-1024, -1024, 0);
		}

		public static bool IsVisible(CPos pos, float scaleX, float scaleY)
		{
			var diff = pos - LookAt;
			var halfzoomY = (int)(CurrentZoom * 512);
			var halfzoomX = halfzoomY * WindowInfo.Ratio + 1024;
			halfzoomY += 1024;

			return diff.X - scaleX < halfzoomX && diff.X + scaleX > -halfzoomX && diff.Y - scaleY < halfzoomY && diff.Y + scaleY > -halfzoomY;
		}

		public static void Reset(bool values = true)
		{
			if (values)
			{
				LookAt = CPos.Zero;
				Locked = false;
				CurrentZoom = DefaultZoom;
			}

			calculatePosition();
			calculateScale();
			updateView();
		}

		public static void Zoom(float add, bool ignoreLock = false)
		{
			if (!ignoreLock && Locked || add == 0)
				return;

			var oldZoom = CurrentZoom;

			CurrentZoom += add;
			if (CurrentZoom < DefaultZoom)
				CurrentZoom = DefaultZoom;
			else if (CurrentZoom > DefaultZoom * 1.5f)
				CurrentZoom = DefaultZoom * 1.5f;

			calculateScale();
			updateView();

			WorldRenderer.CheckVisibility(oldZoom, CurrentZoom);
		}

		public static void Move(CPos add, bool ignoreLock = false)
		{
			if (!ignoreLock && Locked || add == CPos.Zero)
				return;

			var oldLookAt = LookAt;

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

			WorldRenderer.CheckVisibility(oldLookAt, LookAt);
		}

		public static void Position(CPos pos, bool ignoreLock = false)
		{
			if (!ignoreLock && Locked || LookAt == pos)
				return;

			var oldLookAt = LookAt;
			LookAt = pos;

			calculatePosition();
			updateView();

			WorldRenderer.CheckVisibility(oldLookAt, LookAt);
		}

		static void calculatePosition()
		{
			var look = -LookAt.ToVector();
			Matrix4.CreateTranslation(look.X, look.Y, float.MaxValue, out view);

			VisibilitySolver.LookAtUpdated();
		}

		static void calculateScale()
		{
			// cast to [-1;1] | we use 0f to stop things glitching out of sight
			projection = Matrix4.CreateScale(2 / CurrentZoom / WindowInfo.Ratio, 2 / CurrentZoom, 0f);

			VisibilitySolver.ZoomUpdated();
		}

		static void updateView()
		{
			Matrix = view * projection;

			MouseInput.RecalculateMousePosition();
		}
	}
}
