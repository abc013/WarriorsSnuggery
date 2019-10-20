/*
 * User: Andreas
 * Date: 30.09.2017
 * 
 */
using System;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Maps;
using WarriorsSnuggery.Objects;
using WarriorsSnuggery.UI;
using WarriorsSnuggery.Spells;

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

	public enum GameState
	{
		UNKNOWN,
		VICTORY,
		DEFEAT,
		NONE
	}

	public sealed class Game : ITick, IDisposable
	{
		public Random SharedRandom;

		public readonly ScreenControl ScreenControl;
		public readonly World World;
		public readonly Window Window;

		public readonly GameStatistics OldStatistics;
		public readonly GameStatistics Statistics;
		public readonly MapInfo MapType;
		public readonly GameType Type;
		public readonly GameMode Mode;
		public readonly int Seed;

		public readonly SpellManager SpellManager;

		readonly TextLine tick;
		readonly TextLine render;
		readonly TextLine visibility;
		readonly TextLine version;

		readonly TextLine infoText;
		int infoTextDuration;

		public uint LocalTick;
		public uint LocalRender;

		public bool Paused;
		public bool Editor;

		public bool Finished;
		public GameState State;

		bool instantExit;
		GameType instantExitType;

		public int Teams;

		public uint NextObjectID { get { return nextObjectID++; } }
		uint nextObjectID;

		public Game(GameStatistics statistics, MapInfo map, int seed = -1)
		{
			Window = Window.Current;
			MapType = map;

			// If seed negative, calculate it.
			Seed = seed < 0 ? statistics.Seed + statistics.Level : seed;
			SharedRandom = new Random(Seed);

			// In case of death, use this statistic.
			OldStatistics = statistics.Copy();
			// In case of success, use this statistic.
			Statistics = statistics;

			SpellManager = new SpellManager(this);

			Type = MapType.DefaultType;
			Mode = MapType.DefaultModes[SharedRandom.Next(MapType.DefaultModes.Length)];

			Editor = Type == GameType.EDITOR;

			// Determine state of the game
			if (Type != GameType.NORMAL && Type != GameType.TEST && Type != GameType.TUTORIAL)
				State = GameState.NONE;
			else
				State = GameState.UNKNOWN;

			ScreenControl = new ScreenControl(this);

			World = new World(this, Seed, Statistics);

			var corner = (int)(WindowInfo.UnitWidth / 2 * 1024);
			version = new TextLine(new CPos(corner, 6192, 0), IFont.Pixel16, TextLine.OffsetType.RIGHT);
			version.SetColor(Color.Yellow);
			version.SetText(Settings.Version);

			visibility = new TextLine(new CPos(corner, 6692, 0), IFont.Pixel16, TextLine.OffsetType.RIGHT);
			tick = new TextLine(new CPos(corner, 7692, 0), IFont.Pixel16, TextLine.OffsetType.RIGHT);
			render = new TextLine(new CPos(corner, 7192, 0), IFont.Pixel16, TextLine.OffsetType.RIGHT);

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
			var timer = Timer.Start();

			MasterRenderer.ResetRenderer(this);

			World.Load();
			World.AddObjects();

			ScreenControl.Load();

			timer.StopAndWrite("Loading Game");

			if (Window.FirstTick && Settings.FirstStarted)
			{
				Pause(true);
				ChangeScreen(ScreenType.START);
			}
			else
			{
				ChangeScreen(ScreenType.DEFAULT);
			}

			Camera.Reset();
		}

		public void Pause(bool paused)
		{
			Paused = paused;
			MasterRenderer.PauseSequences = Paused;
			Camera.Locked = Paused;
		}

		public void Tick()
		{
			if (Finished && instantExit)
			{
				Window.Current.NewGame(Statistics, instantExitType);
				return;
			}

			var watch = Timer.Start();

			if (LocalTick == 1)
			{
				// Check visibility the first time, one tick after because we have to check for shroud first
				WorldRenderer.CheckVisibilityAll();
			}

			LocalTick++;

			if (!Paused && !Finished)
			{
				// screen control
				if (ScreenControl.FocusedType != ScreenType.DEFEAT)
				{
					if (KeyInput.IsKeyDown(Settings.Key("Pause"), 10))
					{
						Pause(true);
						ChangeScreen(ScreenType.PAUSED);
					}
					if (KeyInput.IsKeyDown("escape", 10))
					{
						Pause(true);
						ChangeScreen(ScreenType.MENU);
					}

					CheckVictory();
				}

				// party mode
				if (Settings.PartyMode)
				{
					var sin1 = (float)Math.Sin(LocalTick / 8f) / 2 + 0.25f;
					var sin2 = (float)Math.Sin(LocalTick / 8f + 2 * Math.PI / 3) / 2 + 0.25f;
					var sin3 = (float)Math.Sin(LocalTick / 8f + 4 * Math.PI / 3) / 2 + 0.25f;

					WorldRenderer.Ambient = new Color(sin1 * WorldRenderer.Ambient.R, sin2 * WorldRenderer.Ambient.R, sin3 * WorldRenderer.Ambient.R);
				}

				// camera input
				if (!ScreenControl.CursorOnUI() && !(Camera.LockedToPlayer && World.PlayerAlive))
				{
					var mouse = MouseInput.WindowPosition;

					var add = CPos.Zero;

					if (KeyInput.IsKeyDown(Settings.Key("CameraUp"), 0) || (mouse.Y < 0 && mouse.Y < -Camera.DefaultZoom * 512 + 64 * Settings.EdgeScrolling))
						add = new CPos(add.X, add.Y - 1, 0);

					if (KeyInput.IsKeyDown(Settings.Key("CameraDown"), 0) || (mouse.Y > 0 && mouse.Y > Camera.DefaultZoom * 512 - 64 * Settings.EdgeScrolling))
						add = new CPos(add.X, add.Y + 1, 0);

					if (KeyInput.IsKeyDown(Settings.Key("CameraRight"), 0) || (mouse.X > 0 && mouse.X > Camera.DefaultZoom * WindowInfo.Ratio * 512 - 64 * Settings.EdgeScrolling))
						add = new CPos(add.X + 1, add.Y, 0);

					if (KeyInput.IsKeyDown(Settings.Key("CameraLeft"), 0) || (mouse.X < 0 && mouse.X < -Camera.DefaultZoom * WindowInfo.Ratio * 512 + 64 * Settings.EdgeScrolling))
						add = new CPos(add.X - 1, add.Y, 0);

					if (add != CPos.Zero)
						Camera.Move(add);
				}

				// Key input
				if (KeyInput.IsKeyDown(Settings.Key("CameraLock"), 5))
				{
					Camera.LockedToPlayer = !Camera.LockedToPlayer;
				}

				if (KeyInput.IsKeyDown("altright", 5))
				{
					Settings.PartyMode = !Settings.PartyMode;
				}

				if (KeyInput.IsKeyDown("altleft", 0))
				{
					if (KeyInput.IsKeyDown("v", 5) && Type != GameType.EDITOR)
					{
						World.LocalPlayer.Health.HP = 0;
					}
					if (KeyInput.IsKeyDown("b", 5) && Type != GameType.EDITOR)
					{
						World.LocalPlayer.Health.HP += 100;
					}
					if (KeyInput.IsKeyDown("n", 5))
					{
						Statistics.Mana += 100;
						if (Statistics.Mana > Statistics.MaxMana)
							Statistics.Mana = Statistics.MaxMana;
					}
					if (KeyInput.IsKeyDown("m", 5))
					{
						Statistics.Money += 100;
					}
					if (KeyInput.IsKeyDown("comma", 5))
					{
						Settings.EnableInfoScreen = !Settings.EnableInfoScreen;
					}
					if (KeyInput.IsKeyDown("period", 5))
					{
						World.ShroudLayer.AllRevealed = true;
					}
					if (KeyInput.IsKeyDown("x", 5))
					{
						Screen defaultScreen;
						if (Editor)
						{
							if (Type == GameType.MENU || Type == GameType.MAINMENU)
								defaultScreen = null;
							else
								defaultScreen = new DefaultScreen(this);
						}
						else
						{
							defaultScreen = new EditorScreen(this);
						}
						Editor = !Editor;

						ScreenControl.NewDefaultScreen(defaultScreen);

						ChangeScreen(ScreenType.DEFAULT);
					}
				}

				// Zooming
				if (!Editor && Type != GameType.EDITOR)
				{
					if (KeyInput.IsKeyDown("controlleft") && MouseInput.IsRightDown)
						Camera.Zoom(Settings.ScrollSpeed / 20 * (4 - (Camera.CurrentZoom - Camera.DefaultZoom) / 2));
					else
						Camera.Zoom(Settings.ScrollSpeed / 20 * (-(Camera.CurrentZoom - Camera.DefaultZoom) / 2));
				}

				SpellManager.Tick();
				World.Tick();
			}

			if (Settings.EnableInfoScreen)
			{
				//memory.SetText("Memory " + (int) (System.Diagnostics.Process.GetCurrentProcess().PrivateMemorySize64 / 1024f) + " KB");
				//memory.SetText("Public Memory " + (int)(GC.GetTotalMemory(false) / 1024f) + " KB");
				visibility.SetText(VisibilitySolver.TilesVisible() + " Tiles visible");

				var tickColor = Color.White;
				if (Window.Current.TPS < Settings.UpdatesPerSecond - 5)
					tickColor = Color.Red;
				else if (Window.Current.TPS > Settings.UpdatesPerSecond + 5)
					tickColor = Color.Green;

				tick.SetColor(tickColor);
				tick.SetText("Tick " + Window.Current.TPS + " @ " + Window.Current.TMS + " ms");

				var renderColor = Color.White;
				if (Window.Current.FPS < Settings.FrameLimiter - 20)
					renderColor = Color.Red;
				else if (Window.Current.FPS > Settings.FrameLimiter + 5)
					renderColor = Color.Green;

				render.SetColor(renderColor);
				render.SetText("Render " + Window.Current.FPS + " @ " + Window.Current.FMS + " ms");
			}

			if (infoTextDuration-- < 90)
			{
				infoText.Position -= new CPos(48, 0, 0);
			}

			ScreenControl.Tick();

			var ms = watch.Stop();
			if (LocalTick % 4 == 0)
				Log.WritePerformance(ms, " Tick " + LocalTick);

			if (ScreenControl.FocusedType == ScreenType.START)
				Pause(true);
		}

		public void CheckVictory()
		{
			switch (Mode)
			{
				// FIND_EXIT and TUTORIAL will meet conditions when entering the exit
				default:
					break;
				// TODO not yet implemented.
				// When no enemies are present, won
				case GameMode.TOWER_DEFENSE:
				case GameMode.WIPE_OUT_ENEMIES:
					var actor = World.Actors.Find(a => !(a.Team == Actor.PlayerTeam || a.Team == Actor.NeutralTeam));

					if (actor == null)
						VictoryConditionsMet();
					break;
			}
		}

		public void VictoryConditionsMet()
		{
			State = GameState.VICTORY;
			Finish();

			Statistics.Level++;
			if (World.PlayerAlive && World.LocalPlayer.Health != null)
				Statistics.Health = World.LocalPlayer.Health.HP;

			ChangeScreen(ScreenType.VICTORY);
		}

		public void DefeatConditionsMet()
		{
			State = GameState.DEFEAT;
			Finish();

			ChangeScreen(ScreenType.DEFEAT);
		}

		// Instant travel to next level
		public void InstantLevelChange(GameType newType)
		{
			Finish();

			instantExit = true;
			instantExitType = newType;
		}

		public void Finish()
		{
			Finished = true;
			Pause(true);
		}

		public void ChangeScreen(ScreenType screen)
		{
			ScreenControl.ShowScreen(screen);

			if (screen == ScreenType.DEFEAT)
				ScreenControl.NewDefaultScreen(ScreenControl.Focused);
		}

		public void RefreshSaveGameScreens()
		{
			ScreenControl.RefreshSaveGameScreens();
		}

		public void RenderDebug()
		{
			if (Settings.EnableInfoScreen)
			{
				version.Render();
				visibility.Render();
				tick.Render();
				render.Render();
			}

			if (infoTextDuration > 0) infoText.Render();
		}

		public void Dispose()
		{
			if (!Finished)
				throw new Exception("Game has not been finished before dispose call.");

			World.Dispose();

			ScreenControl.DisposeScreens();

			version.Dispose();
			visibility.Dispose();
			tick.Dispose();
			render.Dispose();
			infoText.Dispose();

			WorldRenderer.ClearRenderLists();
			UIRenderer.ClearRenderLists();

			VisibilitySolver.Reset();

			GC.Collect();
		}
	}
}
