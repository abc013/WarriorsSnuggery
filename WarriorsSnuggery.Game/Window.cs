using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Desktop;
using System;
using WarriorsSnuggery.Audio;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery
{
	public static class WindowInfo
	{
		public static int Height;
		public static int Width;

		public static int ScreenHeight;
		public static int ScreenWidth;

		public static int ScreenRefreshRate;

		public static float Ratio => Width / (float)Height;

		public const float UnitHeight = Camera.DefaultZoom;

		public static float UnitWidth { get => Camera.DefaultZoom * Ratio; private set { } }

		public static bool Focused = true;
	}

	public class Window : GameWindow
	{
		static Window current;

		public static string StringInput;
		public static Keys KeyInput;

		public static uint GlobalTick;
		public static uint GlobalRender;

		public static bool Ready;
		public static bool Stopped;

		public static long TMS;
		public static double TPS;

		public static long FMS;
		public static double FPS;

		readonly Timer timer;

		public Window(GameWindowSettings settings1, NativeWindowSettings settings2) : base(settings1, settings2)
		{
			current = this;
			CursorVisible = false;

			// Initialize values
			unsafe
			{
				var mode = GLFW.GetVideoMode(CurrentMonitor.ToUnsafePtr<Monitor>());
				WindowInfo.ScreenWidth = mode->Width;
				WindowInfo.ScreenHeight = mode->Height;
				WindowInfo.ScreenRefreshRate = mode->RefreshRate;
			}
			setScreen();
			setVSync();

			timer = Timer.Start();
			timer.Stop();
		}

		public static void CloseWindow()
		{
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

				ClientRectangle = new Box2i(0, 0, WindowInfo.ScreenWidth, WindowInfo.ScreenHeight);
			}
			else
			{
				WindowBorder = WindowBorder.Fixed;
				WindowState = WindowState.Normal;
				var offsetX = (WindowInfo.ScreenWidth - Settings.Width) / 2;
				var offsetY = (WindowInfo.ScreenHeight - Settings.Height) / 2;
				ClientRectangle = new Box2i(offsetX, offsetY + 1, Settings.Width + offsetX, Settings.Height + offsetY);
			}

			RenderFrequency = Settings.FrameLimiter == 0 ? WindowInfo.ScreenRefreshRate : Settings.FrameLimiter;
			WindowInfo.Width = ClientRectangle.Size.X;
			WindowInfo.Height = ClientRectangle.Size.Y;

			ColorManager.WindowRescaled();

			MasterRenderer.UpdateView();
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

			MasterRenderer.Initialize();
			SpriteManager.InitSheets();

			timer.Restart();

			FontManager.Load();

			timer.StopAndWrite("Loading Fonts");
			timer.Restart();

			AudioController.Load();

			timer.StopAndWrite("Loading Sound");
			timer.Restart();

			GameController.Load();

			timer.StopAndWrite("Loading Rules");
			timer.Restart();

			SpriteManager.CreateTextures();

			timer.StopAndWrite("Loading Textures");

			Ready = true;
			Console.WriteLine(" Done!");
		}

		protected override void OnUpdateFrame(FrameEventArgs e)
		{
			if (!Ready)
				return;

			if (GlobalTick % 20 == 0)
				timer.Restart();

			MouseInput.State = MouseState;
			WarriorsSnuggery.KeyInput.State = KeyboardState;

			GameController.Tick();
			AudioController.Tick();

			MouseInput.WheelState = 0;
			StringInput = string.Empty;
			KeyInput = Keys.End;

			if (GlobalTick % 20 == 0)
			{
				TMS = timer.Stop();
				TPS = 1 / e.Time;

				Log.WritePerformance(TMS, " tick " + GlobalTick);
			}

			GlobalTick++;
		}

		protected override void OnRenderFrame(FrameEventArgs e)
		{
			if (!Ready || Stopped)
				return;

			if (GlobalRender % 20 == 0)
				timer.Restart();

			MasterRenderer.Render();

			lock (MasterRenderer.GLLock)
			{
				SwapBuffers();
			}

			if (GlobalRender % 20 == 0)
			{
				FMS = timer.Stop();
				FPS = 1 / e.Time;

				Log.WritePerformance(FMS, " render " + GlobalRender);
				Title = Program.Title + " | " + MasterRenderer.RenderCalls + " Calls | " + MasterRenderer.Batches + " Batches | " + MasterRenderer.BatchCalls + " BatchCalls";
			}

			GlobalRender++;
		}

		protected override void OnFocusedChanged(FocusedChangedEventArgs e)
		{
			WindowInfo.Focused = e.IsFocused;
			if (!e.IsFocused && !Program.IsDebug && Ready)
				GameController.Pause();
		}

		public override void Close()
		{
			Stopped = true;

			UITextureManager.Dispose();

			WorldRenderer.BatchRenderer.Dispose();
			UIRenderer.BatchRenderer.Dispose();
			MasterRenderer.Dispose();

			SpriteManager.DeleteTextures();

			FontManager.Dispose();

			base.Close();
		}

		protected override void OnMouseMove(MouseMoveEventArgs e)
		{
			MouseInput.UpdateMousePosition(e.Position.X, e.Position.Y);
		}

		protected override void OnMouseWheel(MouseWheelEventArgs e)
		{
			MouseInput.WheelState = (int)e.OffsetY;
		}

		protected override void OnTextInput(TextInputEventArgs e)
		{
			StringInput = e.AsString;
		}

		protected override void OnKeyDown(KeyboardKeyEventArgs e)
		{
			KeyInput = e.Key;

			if (e.Key == Keys.N)
				AudioController.Music.Next();

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
