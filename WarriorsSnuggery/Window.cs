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
		public static Window Current;
		public static char CharInput;

		public static uint GlobalTick;
		public static uint GlobalRender;

		public static bool Ready;
		public static bool Exiting;
		public static bool FirstTick = true;

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

			ColorManager.WindowRescaled();

			MasterRenderer.UpdateView();
		}

		protected override void OnLoad(EventArgs e)
		{
			Console.Write("Loading...");

			MasterRenderer.Initialize();

			base.OnLoad(e);

			var font = Timer.Start();

			Icon = new Icon(FileExplorer.Misc + "/warsnu.ico");
			IFont.LoadFonts();
			IFont.InitializeFonts();
			CharManager.Initialize();

			font.StopAndWrite("Loading Fonts");
			var watch = Timer.Start();

			RuleLoader.LoadRules();
			RuleLoader.LoadUIRules();

			MapCreator.LoadTypes(FileExplorer.Maps, "maps.yaml");

			GameSaveManager.Load();
			GameSaveManager.DefaultStatistic = GameStatistics.LoadGameStatistic("DEFAULT");

			NewGame(new GameStatistics(GameSaveManager.DefaultStatistic), GameType.MAINMENU);

			watch.StopAndWrite("Loading Rules");

			Ready = true;
			Console.WriteLine(" Done!");
			Console.WriteLine("Textures: " + TextureManager.TextureCount);

			// For multithreads
			//IGraphicsContext context2 = new GraphicsContext(GraphicsMode.Default, this.WindowInfo);
			//context2.MakeCurrent(WindowInfo);
		}

		public float TPS;
		public float TMS;
		protected override void OnUpdateFrame(FrameEventArgs e)
		{
			if (!Ready)
				return;

			KeyInput.Tick();
			MouseInput.Tick();

			if (GlobalTick % 20 == 0)
			{
				TPS = (float)Math.Round(1 / e.Time, 1);
				TMS = (float)Math.Round(e.Time * 1000, 1);
			}

			Game.Tick();

			CharInput = '';
			FirstTick = false;

			if (KeyInput.IsKeyDown(Key.F4) && (KeyInput.IsKeyDown(Key.AltLeft) || KeyInput.IsKeyDown(Key.AltRight)))
				Exit();

			GlobalTick++;
		}

		public float FPS;
		public float FMS;

		protected override void OnRenderFrame(FrameEventArgs e)
		{
			if (!Ready || Exiting)
				return;

			Timer watch = null;
			if (GlobalRender % 20 == 0)
			{
				FPS = (float)Math.Round(1 / e.Time, 1);
				FMS = (float)Math.Round(e.Time * 1000, 1);
				watch = Timer.Start();
			}

			MasterRenderer.Render();

			if (GlobalRender % 20 == 0)
				watch.StopAndWrite("render" + GlobalRender);

			lock (MasterRenderer.GLLock)
			{
				SwapBuffers();
			}

			GlobalRender++;
		}

		protected override void OnFocusedChanged(EventArgs e)
		{
			base.OnFocusedChanged(e);

			if (!Focused && !Program.isDebug && Ready)
			{
				if (!Game.Paused)
					Game.ChangeScreen(UI.ScreenType.PAUSED);

				Game.Pause(true);
			}
		}

		public void NewGame(GameStatistics stats, GameType type = GameType.NORMAL, bool sameSeed = false, MapInfo custom = null, bool loadStatsMap = false)
		{
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

			Camera.Reset();
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

			TerrainSpriteManager.DeleteTexture();
			SpriteManager.DeleteTextures();

			IImage.DisposeImages();
			IFont.DisposeFonts();

			base.Exit();
			watch.StopAndWrite("Disposing");
		}

		protected override void OnMouseMove(MouseMoveEventArgs e)
		{
			MouseInput.UpdateMousePosition(new MPos(e.Position.X, e.Position.Y));
		}

		protected override void OnKeyDown(KeyboardKeyEventArgs e)
		{
			KeyInput.KeyPressed(e);
			var str = e.Key.ToString();
			if (str.Length == 1)
			{
				CharInput = str[0];
				if (!e.Shift)
					CharInput = char.ToLower(CharInput);
			}
		}
	}
}
