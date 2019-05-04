using OpenTK;

namespace WarriorsSnuggery
{
	public static class Camera
	{
		public static CPos LookAt = CPos.Zero;

		public static bool Locked;
		public static bool LockedToPlayer = true;

		public const float DefaultZoom = 16f;
		public static float CurrentZoom = DefaultZoom;

		static Matrix4 Projection = Matrix4.Zero;
		static Matrix4 View = Matrix4.Zero;

		public static Matrix4 Matrix = Matrix4.Zero;

		static CPos bounds = CPos.Zero;

		public static void SetBounds(CPos size)
		{
			bounds = size + new CPos(-1024, -1024,0);
		}

		public static bool IsVisible(Vector4 pos, float scaleX, float scaleY)
		{
			return IsVisible(GLPos.ToCPos(pos), scaleX, scaleY);
		}

		public static bool IsVisible(CPos pos, float scaleX, float scaleY)
		{
			var diff = pos - LookAt;
			var halfzoomY = ((int)CurrentZoom * 1024) / 2 + 1024;
			var halfzoomX = halfzoomY * WindowInfo.Ratio;

			return diff.X - scaleX < halfzoomX && diff.X + scaleX > -halfzoomX && diff.Y - scaleY < halfzoomY && diff.Y + scaleY > -halfzoomY;
		}

		public static void Reset()
		{
			LookAt = CPos.Zero;
			Locked = false;
			CurrentZoom = DefaultZoom;
		}

		public static void Zoom(float add, bool ignoreLock = false)
		{
			if(!ignoreLock && Locked)
				return;

			var newzoom = CurrentZoom + (add / 4f);
			if (newzoom < DefaultZoom || newzoom > DefaultZoom * 1.5f)
				return;

			CurrentZoom = newzoom;
			var window = Window.Current;

			Projection = Matrix4.CreateScale(1/ newzoom * 2 / WindowInfo.Ratio, 1 / newzoom * 2, 0f); //HACK: we use 0f to stop things glitching out of sight
			//Matrix4.CreateOrthographic(newzoom / 2 * WindowInfo.Ratio, newzoom / 2, 100f,0, out Projection);
			if (!add.Equals(0))
				UpdateView();
		}
		
		public static void Move(CPos add, bool ignoreLock = false)
		{
			if(!ignoreLock && (Locked || add == CPos.Zero))
				return;

			LookAt = new CPos(LookAt.X + (int) (Settings.ScrollSpeed * 20 * add.X), LookAt.Y + (int) (Settings.ScrollSpeed * 20 * add.Y), 0);

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

			UpdateView();
		}

		public static void Position(CPos pos, bool ignoreLock = false)
		{
			if(!ignoreLock && Locked)
				return;

			LookAt = pos;

			UpdateView();
		}

		public static void UpdateView()
		{
			var look = new Vector3(-LookAt.ToVector() / CurrentZoom) * 2;
			look.X /= WindowInfo.Ratio;
			look.Z = 0;

			Matrix4.CreateTranslation(ref look, out View);
			WorldRenderer.CheckObjectVisibility();
			Matrix = Projection * View;
		}
	}
}
