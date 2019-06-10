using System;
using WarriorsSnuggery.Objects;
using WarriorsSnuggery.UI;
using WarriorsSnuggery.Maps;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery
{
	public enum GameType
	{
		NORMAL,
		EDITOR,
		MENU,
		MAINMENU,
		TUTORIAL,
		TEST
	}

	public enum GameMode
	{
		TOWER_DEFENSE,
		WIPE_OUT_ENEMIES,
		FIND_EXIT,
		TUTORIAL,
		NONE
	}

	public sealed class Game : ITick, IDisposable
	{
		public Random SharedRandom = new Random();
		
		public readonly ScreenControl ScreenControl;

		public uint LocalTick;
		public uint LocalRender;

		public static bool Paused; // used in sprites, should remove static access...?
		public bool End;
		public GameType NewGameType;
		public bool Editor;

		public uint NextObjectID { get { return nextObjectID++; } }
		uint nextObjectID;

		public readonly World World;

		public readonly Window Window;

		public readonly GameStatistics OldStatistics;
		public readonly GameStatistics Statistics;
		public readonly MapType MapType;
		public readonly GameType Type;
		public readonly GameMode Mode;
		public readonly int Seed;
		
		readonly TextLine tick;
		readonly TextLine render;
		readonly TextLine memory;
		readonly TextLine version;

		readonly TextLine infoText;
		int infoTextDuration;

		public Game(GameStatistics statistics, MapType map, int seed = -1)
		{
			Window = Window.Current;

			if (seed < 0) seed = statistics.Seed + statistics.Level;
			Seed = seed;
			MapType = map;
			OldStatistics = statistics.Copy();
			Statistics = statistics;

			Type = MapType.DefaultType;
			Mode = MapType.DefaultModes[new Random(seed).Next(MapType.DefaultModes.Length)];

			if (Type == GameType.EDITOR)
				Editor = true;

			ScreenControl = new ScreenControl(this);

			World = new World(this, seed, Statistics.Level, Statistics.Difficulty);

			MasterRenderer.ResetRenderer(this);

			var corner = (int) (WindowInfo.UnitWidth / 2 * 1024);
			version = new TextLine(new CPos(corner, 6192,0), IFont.Pixel16, TextLine.OffsetType.RIGHT);
			version.SetColor(Color.Yellow);
			version.SetText(Settings.Version);

			memory = new TextLine(new CPos(corner, 6692,0), IFont.Pixel16, TextLine.OffsetType.RIGHT);
			tick = new TextLine(new CPos(corner, 7692,0), IFont.Pixel16, TextLine.OffsetType.RIGHT);
			render = new TextLine(new CPos(corner, 7192,0), IFont.Pixel16, TextLine.OffsetType.RIGHT);

			infoText = new TextLine(new CPos(-corner + 1024, 7192, 0), IFont.Pixel16);
		}

		public void AddInfoMessage(int duration, string text)
		{
			var corner = (int)(WindowInfo.UnitWidth / 2 * 1024);
			infoText.Position = new CPos(-corner + 1024, 7192, 0);
			infoText.WriteText(text);
			infoTextDuration = duration;
		}

		public void Load()
		{
			var watch = StopWatch.StartNew();

			World.Load();
			World.AddObjects();

			ScreenControl.Load();

			watch.Stop();
			Log.WritePerformance(watch.ElapsedMilliseconds, "Loading Game");

			if (Window.FirstTick && Settings.FirstStarted)
			{
				ChangeScreen(ScreenType.START);
			}
			else
			{
				Pause(false);
			}
		}

		public void Pause()
		{
			Pause(!Paused);
		}

		public void Pause(bool paused)
		{
			Paused = paused;
			Camera.Locked = Paused;
			if (!Paused)
				ChangeScreen(ScreenType.DEFAULT);
		}

		public void Tick()
		{
			if (End)
			{
				Window.NewGame(Statistics, NewGameType);
				return;
			}
			var watch = new StopWatch();
			watch.Start();

			LocalTick++;

			if (!Paused)
			{
				// screen control
				if (ScreenControl.FocusedType != ScreenType.FAILURE)
				{
					if (KeyInput.IsKeyDown(Settings.Key("Pause"), 10))
					{
						Pause();
						ChangeScreen(ScreenType.PAUSED);
					}
					if (KeyInput.IsKeyDown("escape", 10))
					{
						Pause();
						ChangeScreen(ScreenType.MENU);
					}
					if (KeyInput.IsKeyDown("altleft", 0) && KeyInput.IsKeyDown("m", 10))
					{
						Screen defaultScreen;
						if(Editor)
						{
							Editor = false;
							if (Type == GameType.MENU || Type == GameType.MAINMENU)
								defaultScreen = null;
							else
								defaultScreen = new DefaultScreen(this);
						}
						else
						{
							Editor = true;
							defaultScreen = new EditorScreen(this);
						}

						ScreenControl.NewDefaultScreen(defaultScreen);

						ChangeScreen(ScreenType.DEFAULT);
					}
					if (WinConditionsMet())
					{
						Pause();
						ChangeScreen(ScreenType.WIN);
					}
				}

				// party mode
				if (Settings.PartyMode)
				{
					var sin1 = (float) Math.Sin(LocalTick / 8f);
					var sin2 = (float) Math.Sin(LocalTick / 8f + 2 * Math.PI / 3);
					var sin3 = (float) Math.Sin(LocalTick / 8f + 4 * Math.PI / 3);

					if (sin1 < 0) sin1 = 0;
					if (sin2 < 0) sin2 = 0;
					if (sin3 < 0) sin3 = 0;

					WorldRenderer.Ambient = new Color(sin1 * WorldRenderer.Ambient.R, sin2 * WorldRenderer.Ambient.R, sin3 * WorldRenderer.Ambient.R);
				}

				// camera input
				if (Editor && !ScreenControl.CursorOnUI())
				{
					var mouse = MouseInput.WindowPosition;

					var add = CPos.Zero;
					if (KeyInput.IsKeyDown(Settings.Key("CameraUp"), 0) || (mouse.Y < 0 && mouse.Y < -Camera.DefaultZoom * 512 + 64 * Settings.EdgeScrolling))
					{
						add = new CPos(add.X, add.Y - 1, 0);
					}
					if (KeyInput.IsKeyDown(Settings.Key("CameraDown"), 0) || (mouse.Y > 0 && mouse.Y > Camera.DefaultZoom * 512 - 64 * Settings.EdgeScrolling))
					{
						add = new CPos(add.X, add.Y + 1, 0);
					}
					if (KeyInput.IsKeyDown(Settings.Key("CameraRight"), 0) || (mouse.X > 0 && mouse.X > Camera.DefaultZoom * WindowInfo.Ratio * 512 - 64 * Settings.EdgeScrolling))
					{
						add = new CPos(add.X + (int)1, add.Y, 0);
					}
					if (KeyInput.IsKeyDown(Settings.Key("CameraLeft"), 0) || (mouse.X < 0 && mouse.X < -Camera.DefaultZoom * WindowInfo.Ratio * 512 + 64 * Settings.EdgeScrolling))
					{
						add = new CPos(add.X - (int)1, add.Y, 0);
					}

					if (add != CPos.Zero)
					{
						Camera.Move(add);
					}
				}

				// Key input
				if (KeyInput.IsKeyDown(Settings.Key("CameraLock"), 10))
				{
					Camera.LockedToPlayer = !Camera.LockedToPlayer;
				}

				if (KeyInput.IsKeyDown("altright", 10))
				{
					Settings.PartyMode = !Settings.PartyMode;
				}

				if (KeyInput.IsKeyDown("altleft", 0))
				{
					if (KeyInput.IsKeyDown("n", 10))
					{
						World.Game.Statistics.Mana += 100;
						if (World.Game.Statistics.Mana > World.Game.Statistics.MaxMana)
							World.Game.Statistics.Mana = World.Game.Statistics.MaxMana;
					}
					if (KeyInput.IsKeyDown("b", 10))
					{
						World.LocalPlayer.Health.HP += 100;
					}
					if (KeyInput.IsKeyDown("v", 10))
					{
						Statistics.Money += 100;
					}
					if (KeyInput.IsKeyDown("comma", 10))
					{
						Settings.EnableInfoScreen = !Settings.EnableInfoScreen;
					}
				}

				// Zooming
				if (!Editor && Type != GameType.EDITOR)
				{
					if (MouseInput.isRightDown)
					{
						Camera.Zoom(Settings.ScrollSpeed / 5 * (4 - (Camera.CurrentZoom - Camera.DefaultZoom) / 2));
					}
					else
					{
						Camera.Zoom(Settings.ScrollSpeed / 5 * (-(Camera.CurrentZoom - Camera.DefaultZoom) / 2));
					}
				}

				World.Tick();
			}

			if (Settings.EnableInfoScreen)
			{
				//memory.SetText("Memory " + (int) (System.Diagnostics.Process.GetCurrentProcess().PrivateMemorySize64 / 1024f) + " KB");
				//memory.SetText("Public Memory " + (int)(GC.GetTotalMemory(false) / 1024f) + " KB");
				memory.SetText("Fields visible " + VisibilitySolver.FieldsVisible() + " Tiles");
				tick.SetColor(Window.Current.TPS < 59 ? new Color(1f, 0.2f, 0.2f) : Color.White);
				tick.SetText("Tick " + LocalTick + " @ " + Window.Current.TPS);

				tick.SetColor(Window.Current.FPS < Settings.FrameLimiter - 10 ? Color.Red : Color.White);
				render.SetText("Render " + LocalTick + " @ " + Window.Current.FPS);
			}

			if (infoTextDuration-- < 90)
			{
				infoText.Position -= new CPos(48, 0, 0);
			}

			ScreenControl.Tick();

			watch.Stop();
			if (LocalTick % 4 == 0)
				Log.WritePerformance(watch.ElapsedMilliseconds, " Tick " + LocalTick);
			

			if (ScreenControl.FocusedType == ScreenType.START)
				Pause(true);
		}

		public bool WinConditionsMet()
		{
			switch(Mode)
			{
				// FIND_EXIT and TUTORIAL will meet conditions when entering the exit
				default:
					return false;
				// TODO not yet implemented.
				case GameMode.TOWER_DEFENSE:
					var actor = World.Actors.Find(a => !(a.Team == Actor.PlayerTeam || a.Team == Actor.NeutralTeam));
					return actor == null;
				// When no enemies are present, won
				case GameMode.WIPE_OUT_ENEMIES:
					var actor2 = World.Actors.Find(a => !(a.Team == Actor.PlayerTeam || a.Team == Actor.NeutralTeam));
					return actor2 == null;
			}
		}

		public void ChangeScreen(ScreenType screen)
		{
			ScreenControl.ShowScreen(screen);

			switch (screen)
			{
				case ScreenType.FAILURE:
					Pause(true);
					ScreenControl.NewDefaultScreen(ScreenControl.Focused);
					break;
			}
		}

		public void RenderDebug()
		{
			if (Settings.EnableInfoScreen)
			{
				version.Render();
				memory.Render();
				tick.Render();
				render.Render();
			}

			if (infoTextDuration > 0) infoText.Render();
		}

		public void Dispose()
		{
			Pause(true);
			if (World.LocalPlayer != null && World.LocalPlayer.Health != null)
			{
				Statistics.Health = World.LocalPlayer.Health.HP;
			}

			World.Dispose();

			ScreenControl.DisposeScreens();

			version.Dispose();
			memory.Dispose();
			tick.Dispose();
			render.Dispose();

			infoText.Dispose();

			WorldRenderer.ClearRenderLists();
			UIRenderer.ClearRenderLists();

			VisibilitySolver.Reset();

			GC.Collect();
		}

		public void RefreshSaveGameScreens()
		{
			ScreenControl.RefreshSaveGameScreens();
		}
	}
}
