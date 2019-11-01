using System;

namespace WarriorsSnuggery.Graphics
{
	public static class ColorManager
	{
		public static float LineWidth
		{
			get
			{
				return lineWidth;
			}
			set
			{
				lineWidth = value;
				MasterRenderer.SetLineWidth(lineWidth);
			}
		}
		static float lineWidth;
		public const float DefaultLineWidth = 2.0f;

		public static void ResetLineWidth()
		{
			LineWidth = DefaultLineWidth;
		}

		public static void Initialize()
		{
			line = new ColoredLineRenderable(Color.White, 1f);
			fullscreen_rect = new ColoredRectRenderable(Color.White, WindowInfo.UnitHeight, DrawMethod.TRIANGLE);
			WindowRescaled();

			filled_rect = new ColoredRectRenderable(Color.White, 1f, DrawMethod.TRIANGLE);
		}

		public static void WindowRescaled()
		{
			fullscreen_rect.SetScale(WindowInfo.Ratio);
		}

		public static void Dispose()
		{
			line.Dispose();
			fullscreen_rect.Dispose();
			filled_rect.Dispose();
		}

		static ColoredLineRenderable line;

		public static void DrawLine(CPos start, CPos end, Color color)
		{
			line.SetScale(end.Dist(start) / 1024f);
			line.SetRotation(new VAngle(0, 0, -start.Angle(end)) + new VAngle(0, 0, 90));
			line.SetPosition(start);
			line.SetColor(color);

			line.Render();
		}

		static ColoredRectRenderable fullscreen_rect;

		public static void DrawFullscreenRect(Color color)
		{
			fullscreen_rect.SetColor(color);

			fullscreen_rect.Render();
		}

		static ColoredRectRenderable filled_rect;

		public static void DrawRect(CPos bottomleft, CPos topright, Color color)
		{
			filled_rect.SetScale(new CPos(Math.Abs(bottomleft.X - topright.X), Math.Abs(bottomleft.Y - topright.Y), 0));
			filled_rect.SetPosition(new CPos((bottomleft.X + topright.X) / 2, (bottomleft.Y + topright.Y) / 2, 0));
			filled_rect.SetColor(color);

			filled_rect.Render();
		}

		public static void DrawQuad(CPos position, int radius, Color color)
		{
			filled_rect.SetScale(radius / 1024f);
			filled_rect.SetPosition(position);
			filled_rect.SetColor(color);

			filled_rect.Render();
		}
	}
}
