using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Desktop;
using System;
using WarriorsSnuggery.Audio;
using WarriorsSnuggery.Graphics;
using OpenTK.Windowing.Common.Input;
using Image = OpenTK.Windowing.Common.Input.Image;
using WarriorsSnuggery.Loader;
using System.ComponentModel;

namespace WarriorsSnuggery
{
	public static class ScreenInfo
	{
		public static int ScreenHeight;
		public static int ScreenWidth;

		public static int ScreenRefreshRate;
	}

	public static class WindowInfo
	{
		public static int Height;
		public static int Width;

		public static float Ratio => Width / (float)Height;

		public const float UnitHeight = Camera.UIZoom;
		public static float UnitWidth => Camera.UIZoom * Ratio;

		public static bool Focused = true;
	}

	public static class PerfInfo
	{
		public static long TMS { get; private set; }
		public static double TPS { get; private set; }

		static readonly double[] lastTPS = new double[9];
		static int tIndex;

		public static void UpdateLoop(long ms, double tps)
		{
			TMS = ms;

			lastTPS[tIndex++ % 9] = TPS;
			TPS = tps;
		}

		public static double AverageTPS()
		{
			var total = TPS;
			foreach (var tps in lastTPS)
				total += tps;

			return total / 10;
		}

		public static long FMS { get; private set; }
		public static double FPS { get; private set; }

		static readonly double[] lastFPS = new double[9];
		static int fIndex;

		public static void RenderLoop(long ms, double fps)
		{
			FMS = ms;

			lastFPS[fIndex++ % 9] = FPS;
			FPS = fps;
		}

		public static double AverageFPS()
		{
			var total = FPS;
			foreach (var fps in lastFPS)
				total += fps;

			return total / 10;
		}
	}

	public class Window : GameWindow
	{
		static Window current;

		public static uint GlobalTick;
		public static uint GlobalRender;

		public static bool Ready;
		public static bool Stopped;
		static bool closeInitiated;

		readonly Timer timer;

		public Window(GameWindowSettings settings1, NativeWindowSettings settings2) : base(settings1, settings2)
		{
			current = this;
			CursorVisible = false;

			// Initialize values
			unsafe
			{
				var mode = GLFW.GetVideoMode(CurrentMonitor.ToUnsafePtr<Monitor>());
				ScreenInfo.ScreenWidth = mode->Width;
				ScreenInfo.ScreenHeight = mode->Height;
				ScreenInfo.ScreenRefreshRate = mode->RefreshRate;
			}

			setScreen();
			setVSync();

			timer = Timer.Start();
			timer.Stop();
		}

		public static void CloseWindow()
		{
			closeInitiated = true;
			current.Close();
		}

		public static void UpdateScreen()
		{
			current.setScreen();
			current.setVSync();
		}

		void setScreen()
		{
			if (Settings.Fullscreen && !Program.NoFullscreen)
			{
				WindowBorder = WindowBorder.Hidden;
				WindowState = WindowState.Fullscreen;

				ClientRectangle = new Box2i(0, 0, ScreenInfo.ScreenWidth, ScreenInfo.ScreenHeight);
			}
			else
			{
				WindowBorder = WindowBorder.Fixed;
				WindowState = WindowState.Normal;
				var offsetX = (ScreenInfo.ScreenWidth - Settings.Width) / 2;
				var offsetY = (ScreenInfo.ScreenHeight - Settings.Height) / 2;
				ClientRectangle = new Box2i(offsetX, offsetY + 1, Settings.Width + offsetX, Settings.Height + offsetY);
			}

			RenderFrequency = Settings.FrameLimiter == 0 ? ScreenInfo.ScreenRefreshRate : Settings.FrameLimiter;
			WindowInfo.Width = ClientRectangle.Size.X;
			WindowInfo.Height = ClientRectangle.Size.Y;

			MasterRenderer.UpdateViewport();
		}

		public static void SetVSync()
		{
			current.setVSync();
		}

		void setVSync()
		{
			VSync = Settings.VSync ? VSyncMode.On : VSyncMode.Off;
		}

		protected override void OnLoad()
		{
			Console.Write("Loading...");

			base.OnLoad();

			KeyInput.SetWindow(this);
			MouseInput.SetWindow(this);

			// Load icon
			var iconData = BitmapLoader.LoadBytes(FileExplorer.FindIn(FileExplorer.Misc, "warsnu"), out var iconWidth, out var iconHeight);
			Icon = new WindowIcon(new Image(iconWidth, iconHeight, iconData));

			MasterRenderer.Initialize();
			SheetManager.InitSheets();

			timer.Restart();
			FontManager.Load();
			timer.StopAndWrite("Loading Fonts");

			timer.Restart();
			AudioController.Load();
			MusicController.Load();
			timer.StopAndWrite("Loading Sound");

			timer.Restart();
			GameController.Load();
			timer.StopAndWrite("Loading Rules");

			SheetManager.FinishSheets();
			MasterRenderer.InitRenderer();

			GameController.CreateFirst();

			Ready = true;
			Console.WriteLine(" Done!");

			if (Program.OnlyLoad)
				Program.Exit();
		}

		protected override void OnUpdateFrame(FrameEventArgs e)
		{
			if (!Ready)
				return;

			timer.Restart();

			GameController.Tick();
			MusicController.Tick();

			MouseInput.Tick();
			KeyInput.Tick();

			PerfInfo.UpdateLoop(timer.Stop(), 1 / e.Time);
			if (Settings.LogTimeMeasuring && GlobalTick % 20 == 0)
				Log.Performance(PerfInfo.TMS, " tick " + GlobalTick);

			GlobalTick++;
		}

		protected override void OnRenderFrame(FrameEventArgs e)
		{
			if (!Ready || Stopped)
				return;

			timer.Restart();

			MasterRenderer.Render();

			lock (MasterRenderer.GLLock)
			{
				SwapBuffers();
			}

			PerfInfo.RenderLoop(timer.Stop(), 1 / e.Time);
			if (GlobalRender % 20 == 0)
			{
				if (Settings.LogTimeMeasuring)
					Log.Performance(PerfInfo.FMS, " render " + GlobalRender);

				if (Settings.DeveloperMode || Program.IsDebug)
					Title = $"{Program.Title} [{MasterRenderer.RenderCalls} Calls, {MasterRenderer.Batches} Batches]";
			}

			GlobalRender++;

			sleep();
		}

		uint lastTick;
		uint lastRender;
		void sleep()
		{
			// Don't use this for multithreading since this method calculates for one thread only.
			if (Settings.EnableMultiThreading || Settings.ThreadSleepFactor == 0f)
				return;

			var tickDiff = GlobalTick - lastTick;
			lastTick = GlobalTick;
			var renderDiff = GlobalRender - lastRender;
			lastRender = GlobalRender;

			var sleepTime = ((int)(1000 * Settings.ThreadSleepFactor) / Math.Max(Settings.UpdatesPerSecond, Settings.FrameLimiter == 0 ? ScreenInfo.ScreenRefreshRate : Settings.FrameLimiter)) - (int)(PerfInfo.TMS * tickDiff + PerfInfo.FMS * renderDiff);
			if (sleepTime > 0)
				System.Threading.Thread.Sleep(sleepTime);
		}

		protected override void OnFocusedChanged(FocusedChangedEventArgs e)
		{
			WindowInfo.Focused = e.IsFocused;
			if (!e.IsFocused && !Program.IsDebug && Ready)
				GameController.Pause();
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			// When using the close button in windowed mode, this will be false
			if (!closeInitiated)
				Program.Exit();

			base.OnClosing(e);
		}

		protected override void Dispose(bool disposing)
		{
			Stopped = true;

			UISpriteManager.Dispose();

			MasterRenderer.Dispose();

			SheetManager.DeleteSheets();

			FontManager.Dispose();

			base.Dispose(disposing);
		}

		protected override void OnMouseMove(MouseMoveEventArgs e)
		{
			MouseInput.UpdateMousePosition(e.Position.X, e.Position.Y);
		}

		protected override void OnMouseWheel(MouseWheelEventArgs e)
		{
			MouseInput.UpdateWheelState((int)e.OffsetY);
		}

		protected override void OnTextInput(TextInputEventArgs e)
		{
			KeyInput.Text = e.AsString;
		}

		protected override void OnKeyDown(KeyboardKeyEventArgs e)
		{
			if (e.Key == Keys.N && !MusicController.SongLooping)
				MusicController.NextSong();

			if (e.Alt && e.Key == Keys.F4)
				Program.Exit();

			if (e.Control && e.Key == Keys.P)
			{
				MasterRenderer.CreateScreenshot();
				GameController.AddInfoMessage(150, "Screenshot!");
			}

			GameController.KeyDown(e.Key, e.Control, e.Shift, e.Alt);
		}
	}
}
