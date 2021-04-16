﻿using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI.Objects
{
	public class InfoScreen : ITickRenderable
	{
		readonly UITextLine version;
		readonly UITextLine visibility;
		readonly UITextLine tick;
		readonly UITextLine render;

		public InfoScreen()
		{
			var corner = (int)(WindowInfo.UnitWidth * 512) - 128;
			version = new UITextLine(FontManager.Pixel16, TextOffset.RIGHT)
			{
				Position = new CPos(corner, 6192, 0),
				Color = Color.Yellow
			};
			version.SetText(Settings.Version);
			visibility = new UITextLine(FontManager.Pixel16, TextOffset.RIGHT) { Position = new CPos(corner, 6692, 0) };
			tick = new UITextLine(FontManager.Pixel16, TextOffset.RIGHT) { Position = new CPos(corner, 7692, 0) };
			render = new UITextLine(FontManager.Pixel16, TextOffset.RIGHT) { Position = new CPos(corner, 7192, 0) };
		}

		public void Tick()
		{
			//memory.SetText("Memory " + (int) (System.Diagnostics.Process.GetCurrentProcess().PrivateMemorySize64 / 1024f) + " KB");
			//memory.SetText("Public Memory " + (int)(GC.GetTotalMemory(false) / 1024f) + " KB");
			visibility.SetText(VisibilitySolver.TilesVisible() + " Tiles visible");

			tick.SetColor(getColor(Window.TPS, Settings.UpdatesPerSecond));
			tick.SetText("Tick " + Window.TPS.ToString("F1") + " @ " + Window.TMS.ToString("00") + " ms");

			var frameCount = Settings.FrameLimiter == 0 ? WindowInfo.ScreenRefreshRate : Settings.FrameLimiter;

			render.SetColor(getColor(Window.FPS, frameCount));
			render.SetText("Render " + Window.FPS.ToString("F1") + " @ " + Window.FMS.ToString("00") + " ms");
		}

		public void Render()
		{
			var right = (int)(WindowInfo.UnitWidth * 512);
			ColorManager.DrawRect(new CPos(right, 8192, 0), new CPos(right - 6144, 8192 - 2560, 0), new Color(0, 0, 0, 128));
			
			version.Render();
			visibility.Render();
			tick.Render();
			render.Render();
		}

		Color getColor(double value, double average)
		{
			if (value < average - 5)
				return new Color(255, 192, 192);

			if (value > average + 5)
				return new Color(192, 255, 192);

			return Color.White;
		}
	}
}
