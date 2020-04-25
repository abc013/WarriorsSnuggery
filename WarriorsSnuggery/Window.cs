using OpenToolkit.Mathematics;
using OpenToolkit.Windowing.Common;
using OpenToolkit.Windowing.Common.Input;
using OpenToolkit.Windowing.Desktop;
using System;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery
{
	public static class WindowInfo
	{
		public static int Height;
		public static int Width;

		public static int ScreenHeight;
		public static int ScreenWidth;

		public static float Ratio { get { return Width / (float)Height; } }

		public const float UnitHeight = Camera.DefaultZoom;

		public static float UnitWidth { get { return Camera.DefaultZoom * Ratio; } private set { } }

		public static bool Focused = true;
	}

	public class Window : GameWindow
	{
		static Window current;

		public static string StringInput;
		public static Key KeyInput;

		public static uint GlobalTick;
		public static uint GlobalRender;

		public static bool Ready;
		public static bool Stopped;

		public Window(GameWindowSettings settings1, NativeWindowSettings settings2) : base(settings1, settings2)
		{
			current = this;
			CursorVisible = false;

			// Initialize values TODO
			WindowInfo.ScreenWidth = /*bounds.Width*/3840;
			WindowInfo.ScreenHeight = /*bounds.Height*/2160;
		}

		public static void CloseWindow()
		{
			current.Close();
		}

		public static void UpdateScreen()
		{
			current.SetScreen();
		}

		public void SetScreen()
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
				var offsetX = WindowInfo.ScreenWidth / 2 - Settings.Width;
				var offsetY = WindowInfo.ScreenHeight / 2 - Settings.Height;
				ClientRectangle = new Box2i(offsetX, offsetY + 1, Settings.Width + offsetX, Settings.Height + offsetY);
			}
			WindowInfo.Width = ClientRectangle.Size.X;
			WindowInfo.Height = ClientRectangle.Size.Y;
			// OnResize should be called automatically

			ColorManager.WindowRescaled();

			MasterRenderer.UpdateView();
		}

		protected override void OnResize(ResizeEventArgs e)
		{
			if (e.Height == 0 || e.Width == 0)
				return;

			base.OnResize(e);

			WindowInfo.Height = e.Height;
			WindowInfo.Width = e.Width;
		}

		protected override void OnLoad()
		{
			Console.Write("Loading...");

			base.OnLoad();
			SetScreen();

			MasterRenderer.Initialize();
			SpriteManager.CreateSheet(8);

			var font = Timer.Start();
			//Icon = new WindowIcon(new OpenToolkit.Windowing.Common.Input.Image(FileExplorer.Misc + "/warsnu.ico"));
			Font.LoadFonts();
			Font.InitializeFonts();

			font.StopAndWrite("Loading Fonts");

			var watch2 = Timer.Start();
			AudioController.Load();

			watch2.StopAndWrite("Loading Sound");

			var watch = Timer.Start();
			GameController.Load();

			watch.StopAndWrite("Loading Rules");

			SpriteManager.CreateTextures();

			Ready = true;
			Console.WriteLine(" Done!");
			Console.WriteLine("Textures: " + TextureManager.TextureCount);

			// For multithreads
			//IGraphicsContext context2 = new GraphicsContext(GraphicsMode.Default, this.WindowInfo);
			//context2.MakeCurrent(WindowInfo);
		}

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
			MouseInput.WheelState = 0;
			WarriorsSnuggery.KeyInput.State = KeyboardState;

			GameController.Tick();
			AudioController.Tick();

			StringInput = string.Empty;
			KeyInput = Key.End;

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
			}

			Title = Program.Title + " | " + MasterRenderer.RenderCalls + " Calls | " + MasterRenderer.Batches + " Batches | " + MasterRenderer.BatchCalls + " BatchCalls";
			GlobalRender++;
		}

		protected override void OnFocusedChanged(FocusedChangedEventArgs e)
		{
			WindowInfo.Focused = e.IsFocused;
			if (!e.IsFocused && !Program.isDebug && Ready)
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

			Graphics.Font.DisposeFonts();

			base.Close();
		}

		protected override void OnMouseMove(MouseMoveEventArgs e)
		{
			MouseInput.UpdateMousePosition(e.Position.X, e.Position.Y);
		}

		protected override void OnMouseWheel(MouseWheelEventArgs e)
		{
			MouseInput.WheelState = (int)e.Offset.X;
		}

		protected override void OnTextInput(TextInputEventArgs e)
		{
			StringInput = e.AsString;
		}

		protected override void OnKeyDown(KeyboardKeyEventArgs e)
		{
			KeyInput = e.Key;

			if (e.Key == Key.N)
				AudioController.Music.Next();

			if (e.Alt && e.Key == Key.F4)
				Program.Exit();

			if (e.Control && e.Key == Key.P)
			{
				MasterRenderer.CreateScreenshot();
				GameController.AddInfoMessage(150, "Screenshot!");
			}
		}
	}
}
