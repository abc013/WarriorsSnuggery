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

		public Window(GameWindowSettings settings1, NativeWindowSettings settings2) : base(settings1, settings2)
		{
			current = this;
			CursorVisible = false;

			// Initialize values
			unsafe
			{
				var mode = GLFW.GetVideoMode(CurrentMonitor.ToUnsafePtr<OpenTK.Windowing.GraphicsLibraryFramework.Monitor>());
				WindowInfo.ScreenWidth = mode->Width;
				WindowInfo.ScreenHeight = mode->Height;
				WindowInfo.ScreenRefreshRate = mode->RefreshRate;
			}
			setScreen();
			setVSync();
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

			var watch = Timer.Start();
			//loadIcon();

			FontManager.Load();

			watch.StopAndWrite("Loading Fonts");
			watch.Restart();

			AudioController.Load();

			watch.StopAndWrite("Loading Sound");
			watch.Restart();

			GameController.Load();

			watch.StopAndWrite("Loading Rules");
			watch.Restart();

			SpriteManager.CreateTextures();

			watch.StopAndWrite("Loading Textures");

			Ready = true;
			Console.WriteLine(" Done!");
		}

		//void loadIcon()
		//{
		//	var floatData = BitmapLoader.LoadTexture(FileExplorer.Misc + "/warsnu.png", out var width, out var height);
		//	var byteData = new byte[floatData.Length];
		//	for (int i = 0; i < floatData.Length; i++)
		//		byteData[i] = (byte)(floatData[i] / 255);

		//	Icon = new WindowIcon(new OpenTK.Windowing.Common.Input.Image(width, height, byteData));
		//}

		public static double TPS;
		public static long TMS;
		protected override void OnUpdateFrame(FrameEventArgs e)
		{
			if (!Ready)
				return;

			Timer watch = null;
			if (GlobalTick % 20 == 0)
				watch = Timer.Start();

			MouseInput.State = MouseState;
			WarriorsSnuggery.KeyInput.State = KeyboardState;

			GameController.Tick();
			AudioController.Tick();

			MouseInput.WheelState = 0;
			StringInput = string.Empty;
			KeyInput = Keys.End;

			if (GlobalTick % 20 == 0)
			{
				TPS = 1 / e.Time;
				TMS = watch.Stop();
				Log.WritePerformance(TMS, " tick " + GlobalTick);
			}

			GlobalTick++;
		}

		public static double FPS;
		public static long FMS;
		protected override void OnRenderFrame(FrameEventArgs e)
		{
			if (!Ready || Stopped)
				return;

			Timer watch = null;
			if (GlobalRender % 20 == 0)
				watch = Timer.Start();

			MasterRenderer.Render();

			lock (MasterRenderer.GLLock)
			{
				SwapBuffers();
			}

			if (GlobalRender % 20 == 0)
			{
				FPS = 1 / e.Time;
				FMS = watch.Stop();
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
			MouseInput.WheelState = (int)(MouseState.PreviousScroll.Y - e.OffsetY);
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
