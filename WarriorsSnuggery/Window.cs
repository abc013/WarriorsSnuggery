using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.ES30;
using OpenTK.Input;
using System;
using System.Drawing;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Maps;

namespace WarriorsSnuggery
{
	public static class WindowInfo
	{
		public static int Height;

		public static int Width;

		public static float Ratio { get { return Width / (float)Height; } }

		public const float UnitHeight = Camera.DefaultZoom;

		public static float UnitWidth { get { return Camera.DefaultZoom * Ratio; } private set { } }
	}

	public class Window : GameWindow
	{
		public static Vector ExactMousePosition;

		public static Window Current;
		public static char CharInput;

		public static uint GlobalTick;
		public static uint GlobalRender;

		public static bool Loaded, Exiting, FirstTick = true;

		public Game Game;
		public static int GamesLoaded;

		const string title = "Warrior's Snuggery";

		public Window() : base(Settings.Width, Settings.Height, GraphicsMode.Default, title)
		{
			Current = this;
			SetScreen();
			CursorVisible = Settings.EnableDebug;
		}

		public void SetScreen()
		{
			if (Settings.Fullscreen && !Program.isDebug)
			{
				WindowBorder = WindowBorder.Hidden;
				WindowState = WindowState.Fullscreen;
				Width = ClientRectangle.Width;
				Height = ClientRectangle.Height;
			}
			else
			{
				WindowBorder = WindowBorder.Fixed;
				WindowState = WindowState.Normal;
				var bounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
				X = bounds.Width / 2 - Settings.Width / 2;
				Y = bounds.Height / 2 - Settings.Height / 2;
				Width = Settings.Width;
				Height = Settings.Height;
			}

			WarriorsSnuggery.WindowInfo.Height = Height;
			WarriorsSnuggery.WindowInfo.Width = Width;
			MasterRenderer.UpdateView();
		}

		protected override void OnResize(EventArgs e)
		{
			WarriorsSnuggery.WindowInfo.Height = Height;
			WarriorsSnuggery.WindowInfo.Width = Width;

			MasterRenderer.UpdateView();
		}

		protected override void OnLoad(EventArgs e)
		{
			Console.Write("Loading...");

			MasterRenderer.Initialize();

			base.OnLoad(e);

			var font = System.Diagnostics.Stopwatch.StartNew();
			Icon = new Icon(FileExplorer.Misc + "/warsnu.ico");
			IFont.LoadFonts();
			IFont.InitializeFonts();
			font.Stop();
			CharManager.Initialize();
			Log.WritePerformance(font.ElapsedMilliseconds, "Loading Fonts");
			var watch = Timer.Start();

			RuleLoader.LoadRules();
			RuleLoader.LoadUIRules();

			MapCreator.LoadTypes(FileExplorer.Maps, "pieces.yaml");

			GameSaveManager.Load();
			GameSaveManager.DefaultStatistic = GameStatistics.LoadGameStatistic("DEFAULT");

			NewGame(new GameStatistics(GameSaveManager.DefaultStatistic), GameType.MAINMENU);

			Loaded = true;

			watch.StopAndWrite("Loading Rules");
			Console.WriteLine(" Done!");
			//IGraphicsContext context2 = new GraphicsContext(GraphicsMode.Default, this.WindowInfo);
			//context2.MakeCurrent(WindowInfo);
		}

		public float TPS;
		protected override void OnUpdateFrame(FrameEventArgs e)
		{
			lock (MasterRenderer.GLLock)
			{
				base.OnUpdateFrame(e);
			}

			KeyInput.Tick();
			MouseInput.Tick();

			if (GlobalTick % 20 == 0)
				TPS = (float)Math.Round(1 / e.Time, 1);

			GlobalTick++;

			if (Loaded)
				Game.Tick();

			if (KeyInput.IsKeyDown(Key.F4) && (KeyInput.IsKeyDown(Key.AltLeft) || KeyInput.IsKeyDown(Key.AltRight)))
				Exit();

			CharInput = '';
			FirstTick = false;
		}

		public float FPS;

		protected override void OnRenderFrame(FrameEventArgs e)
		{
			lock (MasterRenderer.GLLock)
			{
				base.OnRenderFrame(e);
			}

			if (GlobalRender % 20 == 0)
				FPS = (float)Math.Round(1 / e.Time, 1);

			GlobalRender++;

			var watch = System.Diagnostics.Stopwatch.StartNew();

			if (!Exiting)
			{
				MasterRenderer.Render();
			}

			watch.Stop();

			lock (MasterRenderer.GLLock)
			{
				SwapBuffers();
			}
		}

		protected override void OnFocusedChanged(EventArgs e)
		{
			base.OnFocusedChanged(e);

			if (!Focused && !Settings.DeveloperMode && Game != null && Loaded)
			{
				if (!Game.Paused)
					Game.ChangeScreen(UI.ScreenType.PAUSED);

				Game.Pause(true);
			}
		}

		public void NewGame(GameStatistics stats, GameType type = GameType.NORMAL, bool sameSeed = false, MapInfo custom = null, bool loadStatsMap = false)
		{
			Camera.Reset();
			if (Game != null)
			{
				Game.Finish();
				Game.Dispose();
			}

			if (loadStatsMap)
			{
				try
				{
					custom = MapInfo.MapTypeFromSave(stats);
				}
				catch (System.IO.FileNotFoundException)
				{
					Log.WriteDebug(string.Format("Unable to load saved map of save '{0}'. Using a random map.", stats.SaveName));
				}
			}

			if (!sameSeed)
			{
				switch (type)
				{
					case GameType.TUTORIAL:
						Game = new Game(stats, custom ?? MapCreator.FindTutorial());
						break;
					case GameType.MENU:
						Game = new Game(stats, custom ?? MapCreator.FindMainMap(stats.Level));
						break;
					case GameType.MAINMENU:
						Game = new Game(stats, custom ?? MapCreator.FindMainMenuMap(stats.Level));
						break;
					default:
						Game = new Game(stats, custom ?? MapCreator.FindMap(stats.Level));
						GamesLoaded++;
						break;
				}
			}
			else
			{
				Game = new Game(stats, custom ?? Game.MapType, Game.Seed);
			}

			Game.Load();
			if (stats.Health > 0 && Game.World.LocalPlayer != null && Game.World.LocalPlayer.Health != null)
				Game.World.LocalPlayer.Health.HP = stats.Health;

			MasterRenderer.UpdateView();
		}

		public override void Exit()
		{
			var watch = Timer.Start();
			Exiting = true;
			lock (MasterRenderer.GLLock)
			{
				GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			}
			Game.Finish();
			Game.Dispose();

			TextureManager.DeleteTextures();
			ColorManager.Dispose();
			CharManager.Dispose();

			MasterRenderer.Dispose();

			ISprite.DisposeSprites();
			IImage.DisposeImages();
			IFont.DisposeFonts();

			base.Exit();
			watch.StopAndWrite("Disposing");
		}

		protected override void OnMouseMove(MouseMoveEventArgs e)
		{
			base.OnMouseMove(e);
			var pos = e.Position;
			ExactMousePosition = VectorConvert.FromScreen(pos.X, pos.Y);

			MouseInput.WindowPosition = VectorConvert.ToCPos(VectorConvert.FromScreen(pos.X, pos.Y) * new Vector(Camera.DefaultZoom, Camera.DefaultZoom, 1, 1));
		}

		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			CharInput = e.KeyChar; //TODO
		}
	}
}
