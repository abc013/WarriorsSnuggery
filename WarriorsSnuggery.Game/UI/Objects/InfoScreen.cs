using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.UI.Objects
{
	public class InfoScreen : ITickRenderable
	{
		public bool Visible => Settings.EnableInfoScreen;

		readonly UIText version;
		readonly UIText debug;

		public InfoScreen()
		{
			var corner = (int)(WindowInfo.UnitWidth * 512) - 128;
			version = new UIText(FontManager.Default, TextOffset.RIGHT)
			{
				Position = new CPos(corner, 6192, 0),
				Color = Color.Yellow
			};
			version.SetText(Settings.Version);
			debug = new UIText(FontManager.Default, TextOffset.RIGHT) { Position = new CPos(corner, 6692, 0) };
		}

		public void Tick()
		{
			if (Visible || Window.GlobalTick % 10 != 0)
				return;

			//memory.SetText("Memory " + (int) (System.Diagnostics.Process.GetCurrentProcess().PrivateMemorySize64 / 1024f) + " KB");
			//memory.SetText("Public Memory " + (int)(GC.GetTotalMemory(false) / 1024f) + " KB");
			debug.SetText(CameraVisibility.TilesVisible() + " Tiles visible");

			var tps = PerfInfo.AverageTPS();
			debug.AddText(getColor(tps, Settings.UpdatesPerSecond) + $"Tick {tps.ToString("F1")} @ {PerfInfo.TMS:00} ms");

			var frameCount = Settings.FrameLimiter == 0 ? ScreenInfo.ScreenRefreshRate : Settings.FrameLimiter;

			var fps = PerfInfo.AverageFPS();
			debug.AddText(getColor(fps, frameCount) + $"Render {fps.ToString("F1")} @ {PerfInfo.FMS:00} ms");
		}

		public void Render()
		{
			if (Visible)
				return;

			var right = (int)(WindowInfo.UnitWidth * 512);
			ColorManager.DrawRect(new CPos(right, 8192, 0), new CPos(right - 6144, 8192 - 2560, 0), new Color(0, 0, 0, 128));
			
			version.Render();
			debug.Render();
		}

		Color getColor(double value, double average)
		{
			if (value < average - 5)
				return new Color(255, 128, 128);

			if (value > average + 5)
				return new Color(128, 255, 128);

			return Color.White;
		}
	}
}
