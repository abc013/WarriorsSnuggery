using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.UI.Objects
{
	public class InfoScreen : ITickRenderable
	{
		public bool Visible => Settings.EnableInfoScreen;

		readonly UIText version;
		readonly UIText visibility;
		readonly UIText tick;
		readonly UIText render;

		public InfoScreen()
		{
			var corner = (int)(WindowInfo.UnitWidth * 512) - 128;
			version = new UIText(FontManager.Default, TextOffset.RIGHT)
			{
				Position = new CPos(corner, 6192, 0),
				Color = Color.Yellow
			};
			version.SetText(Settings.Version);
			visibility = new UIText(FontManager.Default, TextOffset.RIGHT) { Position = new CPos(corner, 6692, 0) };
			tick = new UIText(FontManager.Default, TextOffset.RIGHT) { Position = new CPos(corner, 7692, 0) };
			render = new UIText(FontManager.Default, TextOffset.RIGHT) { Position = new CPos(corner, 7192, 0) };
		}

		public void Tick()
		{
			if (Visible)
				return;

			//memory.SetText("Memory " + (int) (System.Diagnostics.Process.GetCurrentProcess().PrivateMemorySize64 / 1024f) + " KB");
			//memory.SetText("Public Memory " + (int)(GC.GetTotalMemory(false) / 1024f) + " KB");
			visibility.SetText(CameraVisibility.TilesVisible() + " Tiles visible");

			if (Window.GlobalTick % 10 != 0)
				return;

			var tps = PerfInfo.AverageTPS();
			tick.Color = getColor(tps, Settings.UpdatesPerSecond);
			tick.SetText("Tick " + tps.ToString("F1") + " @ " + PerfInfo.TMS.ToString("00") + " ms");

			var frameCount = Settings.FrameLimiter == 0 ? ScreenInfo.ScreenRefreshRate : Settings.FrameLimiter;

			var fps = PerfInfo.AverageFPS();
			render.Color = getColor(fps, frameCount);
			render.SetText("Render " + fps.ToString("F1") + " @ " + PerfInfo.FMS.ToString("00") + " ms");
		}

		public void Render()
		{
			if (Visible)
				return;

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
