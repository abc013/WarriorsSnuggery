using System;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.Graphics
{
	public static class ColorManager
	{

		public static void Initialize()
		{
			line = new ColoredLineRenderable(Color.White, 1f);
			fullscreenrect = new ColoredRectRenderable(Color.White, WindowInfo.UnitWidth, DrawMethod.TRIANGLE);
		}

		static ColoredLineRenderable line;

		public static void DrawLine(CPos start, CPos end, Color color)
		{
			line.setScale(end.GetDistToXY(start) / 1024f);
			line.setRotation(new CPos(0, 0, (int) -start.GetAngleToXY(end) + 90).ToAngle());
			line.setPosition(start);
			line.setColor(Color.Red);

			line.Render();
		}

		static ColoredRectRenderable fullscreenrect;

		public static void DrawFullscreenRect(Color color)
		{
			fullscreenrect.setColor(color);

			fullscreenrect.Render();
		}

		public static void DrawRect()
		{

		}
	}
}
