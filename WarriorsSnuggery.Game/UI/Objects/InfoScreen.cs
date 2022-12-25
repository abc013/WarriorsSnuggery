using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.UI.Objects
{
	public class InfoScreen : ITick, IRenderable
	{
		public bool Visible => Settings.EnableInfoScreen;

		readonly UIText version;
		readonly UIText debug;

		public InfoScreen()
		{
			var corner = (int)(WindowInfo.UnitWidth * 512) - 128;
			version = new UIText(FontManager.Default, TextOffset.RIGHT)
			{
				Position = new UIPos(corner, 5120),
				Color = Color.Yellow
			};
			version.SetText(Settings.Version);
			debug = new UIText(FontManager.Default, TextOffset.RIGHT) { Position = new UIPos(corner, 5120 + 768) };
		}

		public void Tick()
		{
			if (!Visible || Window.GlobalTick % 10 != 0)
				return;

			//memory.SetText("Memory " + (int) (System.Diagnostics.Process.GetCurrentProcess().PrivateMemorySize64 / 1024f) + " KB");
			//memory.SetText("Public Memory " + (int)(GC.GetTotalMemory(false) / 1024f) + " KB");
			debug.SetText($"{MasterRenderer.Batches} Batches");
			debug.AddText($"{CameraVisibility.TilesVisible()} Tiles visible");

			var tps = PerfInfo.AverageTPS();
			debug.AddText(getColor(tps, Settings.UpdatesPerSecond) + $"Tick {tps:00.0} @ {PerfInfo.TMS:00.0} ms");

			var frameCount = Settings.FrameLimiter == 0 ? ScreenInfo.ScreenRefreshRate : Settings.FrameLimiter;

			var fps = PerfInfo.AverageFPS();
			debug.AddText(getColor(fps, frameCount) + $"Render {fps:00.0} @ {PerfInfo.FMS:00.0} ms");
		}

		public void Render()
		{
			if (!Visible)
				return;

			var right = (int)(WindowInfo.UnitWidth * 512);
			ColorManager.DrawRect(new UIPos(right, 8192), new UIPos(right - 6144 - 256, 5120 - 256), new Color(0, 0, 0, 128));
			
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
