/*
 * User: Andreas
 * Date: 30.09.2017
 * 
 */
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

		public static void Reset()
		{
			LookAt = CPos.Zero;
			Locked = false;
			CurrentZoom = DefaultZoom;
			CalculatePosition();
			CalculateScale();
			UpdateView();
		}

		public static void Zoom(float add, bool ignoreLock = false)
		{
			if (!ignoreLock && Locked || add == 0)
				return;

			CurrentZoom += add;
			if (CurrentZoom < DefaultZoom)
				CurrentZoom = DefaultZoom;
			else if (CurrentZoom > DefaultZoom * 1.5f)
				CurrentZoom = DefaultZoom * 1.5f;

			CalculateScale();
			UpdateView();
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

			CalculatePosition();
			UpdateView();
		}

		public static void Position(CPos pos, bool ignoreLock = false)
		{
			if (!ignoreLock && Locked || LookAt == pos)
				return;

			LookAt = pos;

			CalculatePosition();
			UpdateView();
		}

		static void CalculatePosition()
		{
			var look = -LookAt.ToVector(); // TODO why negate?
			Matrix4.CreateTranslation(look.X, look.Y, float.MaxValue, out View);

			// TODO understand OLD CODE
			//var look = new Vector3(-LookAt.ToVector() / new Vector(CurrentZoom, CurrentZoom, CurrentZoom, CurrentZoom)) * 2;
			//look.X /= WindowInfo.Ratio;
			//look.Z = 0;
			//Matrix4.CreateTranslation(ref look, out View);
			//  Why?
			//Matrix = Projection * View;

			if (Window.Current.Game != null)
				VisibilitySolver.LookAtUpdated();
		}

		static void CalculateScale()
		{
			// cast to [-1;1] | we use 0f to stop things glitching out of sight
			Projection = Matrix4.CreateScale(2 / CurrentZoom / WindowInfo.Ratio, 2 / CurrentZoom, 0f);
			// Matrix4.CreateOrthographic(newzoom / 2 * WindowInfo.Ratio, newzoom / 2, 100f,0, out Projection);

			if (Window.Current.Game != null)
				VisibilitySolver.ZoomUpdated();
		}

		public static void UpdateView()
		{
			Matrix = View * Projection;

			if (Window.Current.Game != null)
				WorldRenderer.CheckObjectVisibility();
		}
	}
}
