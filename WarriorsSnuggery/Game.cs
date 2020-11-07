using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using WarriorsSnuggery.Audio;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Maps;
using WarriorsSnuggery.Objects;
using WarriorsSnuggery.Objects.Conditions;
using WarriorsSnuggery.Spells;
using WarriorsSnuggery.UI;
using WarriorsSnuggery.Scripting;

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
		WAVES,
		KILL_ENEMIES,
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

		public readonly GameStatistics OldStatistics;
		public readonly GameStatistics Statistics;
		public readonly MapInfo MapType;
		public readonly GameType Type;
		public readonly GameMode Mode;
		public readonly int Seed;

		public readonly SpellManager SpellManager;
		public readonly ConditionManager ConditionManager;

		readonly WaveController waveController;

		readonly MissionScriptBase script;

		readonly UITextLine tick;
		readonly UITextLine render;
		readonly UITextLine visibility;
		readonly UITextLine version;

		readonly UITextLine infoText;
		int infoTextDuration;

		public uint LocalTick;
		public uint LocalRender;

		public bool Paused;
		public bool Editor;

		public bool Finished;
		public GameState State;

		bool instantExit;
		GameType instantExitType;

		public uint NextActorID { get { return CurrentActorID++; } }
		public uint CurrentActorID;

		public uint NextWeaponID { get { return CurrentWeaponID++; } }
		public uint CurrentWeaponID;

		public Game(GameStatistics statistics, MapInfo map, int seed = -1)
		{
			Log.WriteDebug("Loading new game...");
			Log.DebugIndentation++;

			MapType = map;

			// If seed negative, calculate it.
			Seed = seed < 0 ? statistics.Seed + statistics.Level : seed;
			SharedRandom = new Random(Seed);

			// In case of death, use this statistic.
			OldStatistics = statistics.Copy();
			// In case of success, use this statistic.
			Statistics = statistics;

			Type = MapType.DefaultType;
			Mode = MapType.DefaultModes[SharedRandom.Next(MapType.DefaultModes.Length)];

			Editor = Type == GameType.EDITOR;

			// Determine state of the game
			if (Type != GameType.NORMAL && Type != GameType.TEST && Type != GameType.TUTORIAL)
				State = GameState.NONE;
			else
				State = GameState.UNKNOWN;

			Log.WriteDebug("Editor: " + Editor);
			Log.WriteDebug("GameType: " + Type);

			SpellManager = new SpellManager(this, statistics);
			ConditionManager = new ConditionManager(this);

			ScreenControl = new ScreenControl(this);

			World = new World(this, Seed, Statistics);

			if (!string.IsNullOrEmpty(map.MissionScript))
			{
				var scriptLoader = new MissionScriptLoader(FileExplorer.FindIn(FileExplorer.Maps, map.MissionScript, ".cs"), map.MissionScript);
				script = scriptLoader.Start(this);
			}
			else
				Log.WriteDebug("No mission script existing.");

			if (Mode == GameMode.WAVES)
				waveController = new WaveController(this);

			var corner = (int)(WindowInfo.UnitWidth / 2 * 1024);
			version = new UITextLine(new CPos(corner, 6192, 0), FontManager.Pixel16, TextOffset.RIGHT)
			{
				Color = Color.Yellow
			};
			version.SetText(Settings.Version);

			visibility = new UITextLine(new CPos(corner, 6692, 0), FontManager.Pixel16, TextOffset.RIGHT);
			tick = new UITextLine(new CPos(corner, 7692, 0), FontManager.Pixel16, TextOffset.RIGHT);
			render = new UITextLine(new CPos(corner, 7192, 0), FontManager.Pixel16, TextOffset.RIGHT);

			infoText = new UITextLine(new CPos(-corner + 1024, 7192, 0), FontManager.Pixel16);
		}

		public void AddInfoMessage(int duration, string text)
		{
			var corner = (int)(WindowInfo.UnitWidth / 2 * 1024);
			infoText.Position = new CPos(-corner + 512, 7192, 0);
			infoText.WriteText(text);
			infoTextDuration = duration;
		}

		public void Load()
		{
			var timer = Timer.Start();

			MasterRenderer.ResetRenderer(this);

			World.Load();

			ScreenControl.Load();

			timer.StopAndWrite("Loading Game");

			if (Window.GlobalTick == 0 && Settings.FirstStarted)
				ChangeScreen(ScreenType.START, true);
			else
				ChangeScreen(ScreenType.DEFAULT);

			if (World.LocalPlayer != null && World.LocalPlayer.Health != null && Statistics.Health > 0)
				World.LocalPlayer.Health.RelativeHP = Statistics.Health;

			WorldRenderer.CheckVisibilityAll();
			MasterRenderer.UpdateView();

			if (World.Map.Type.FromSave)
				script?.LoadState(Statistics.ScriptValues);
			else
				script?.OnStart();

			Log.WriteDebug("Loading successful!");
		}

		public void Pause(bool paused)
		{
			Paused = paused;
			AudioController.PauseAll(paused, true);
			MasterRenderer.PauseSequences = Paused;
			Camera.Locked = Paused;
			Log.WriteDebug((paused ? "P" : "Unp") + "aused game.");
		}

		public void Tick()
		{
			if (Finished && instantExit)
			{
				GameController.CreateNext(instantExitType);
				return;
			}

			LocalTick++;

			if (!Paused && !Finished)
			{
				// screen control
				if (ScreenControl.FocusedType != ScreenType.DEFEAT)
					CheckVictory();

				// camera input
				if (!ScreenControl.CursorOnUI() && !(Camera.LockedToPlayer && World.PlayerAlive && !Editor))
				{
					var mouse = MouseInput.WindowPosition;

					var add = CPos.Zero;

					if (KeyInput.IsKeyDown(Settings.GetKey("CameraUp")) || (mouse.Y < 0 && mouse.Y < -Camera.DefaultZoom * 512 + 64 * Settings.EdgeScrolling))
						add = new CPos(add.X, add.Y - 1, 0);

					if (KeyInput.IsKeyDown(Settings.GetKey("CameraDown")) || (mouse.Y > 0 && mouse.Y > Camera.DefaultZoom * 512 - 64 * Settings.EdgeScrolling))
						add = new CPos(add.X, add.Y + 1, 0);

					if (KeyInput.IsKeyDown(Settings.GetKey("CameraRight")) || (mouse.X > 0 && mouse.X > Camera.DefaultZoom * WindowInfo.Ratio * 512 - 64 * Settings.EdgeScrolling))
						add = new CPos(add.X + 1, add.Y, 0);

					if (KeyInput.IsKeyDown(Settings.GetKey("CameraLeft")) || (mouse.X < 0 && mouse.X < -Camera.DefaultZoom * WindowInfo.Ratio * 512 + 64 * Settings.EdgeScrolling))
						add = new CPos(add.X - 1, add.Y, 0);

					if (add != CPos.Zero)
						Camera.Move(add);
				}

				// Zooming
				if (!Editor && Type != GameType.EDITOR)
				{
					if (KeyInput.IsKeyDown(Keys.LeftControl) && MouseInput.IsRightDown)
						Camera.Zoom(Settings.ScrollSpeed / 20 * (4 - (Camera.CurrentZoom - Camera.DefaultZoom) / 2));
					else
						Camera.Zoom(Settings.ScrollSpeed / 20 * (-(Camera.CurrentZoom - Camera.DefaultZoom) / 2));
				}

				// party mode
				if (Settings.PartyMode)
				{
					var sin1 = (float)Math.Sin(LocalTick / 8f) / 2 + 0.8f;
					var sin2 = (float)Math.Sin(LocalTick / 8f + 2 * Math.PI / 3) / 2 + 0.8f;
					var sin3 = (float)Math.Sin(LocalTick / 8f + 4 * Math.PI / 3) / 2 + 0.8f;

					WorldRenderer.Ambient = new Color(sin1, sin2, sin3);
				}

				SpellManager.Tick();
				ConditionManager.Tick();
				World.Tick();

				script?.Tick();

				if (Mode == GameMode.WAVES)
					waveController.Tick();
			}

			if (Settings.EnableInfoScreen)
			{
				//memory.SetText("Memory " + (int) (System.Diagnostics.Process.GetCurrentProcess().PrivateMemorySize64 / 1024f) + " KB");
				//memory.SetText("Public Memory " + (int)(GC.GetTotalMemory(false) / 1024f) + " KB");
				visibility.SetText(VisibilitySolver.TilesVisible() + " Tiles visible");

				var tickColor = Color.White;
				if (Window.TPS < Settings.UpdatesPerSecond - 5)
					tickColor = new Color(256, 192, 192);
				else if (Window.TPS > Settings.UpdatesPerSecond + 5)
					tickColor = new Color(256, 192, 192);

				tick.SetColor(tickColor);
				tick.SetText("Tick " + Window.TPS.ToString("F1") + " @ " + Window.TMS + " ms");

				var renderColor = Color.White;
				if (Window.FPS < Settings.FrameLimiter - 20)
					renderColor = new Color(256, 192, 192);
				else if (Window.FPS > Settings.FrameLimiter + 5)
					renderColor = new Color(256, 192, 192);

				render.SetColor(renderColor);
				render.SetText("Render " + Window.FPS.ToString("F1") + " @ " + Window.FMS + " ms");
			}

			if (infoTextDuration-- < 120)
				infoText.Position -= new CPos(48, 0, 0);

			ScreenControl.Tick();

			if (ScreenControl.FocusedType == ScreenType.START)
				Pause(true);
		}

		public void KeyDown(Keys key, bool isControl, bool isShift, bool isAlt)
		{
			if (ScreenControl.FocusedType != ScreenType.PAUSED && ScreenControl.FocusedType != ScreenType.DEFEAT)
			{
				if (key == Settings.GetKey("Pause") && !isControl)
				{
					ChangeScreen(ScreenType.PAUSED, true);
					return;
				}
			}

			ScreenControl.KeyDown(key, isControl, isShift, isAlt);

			if (Paused || Finished)
				return;

			// screen control
			 if (ScreenControl.FocusedType != ScreenType.DEFEAT)
			{
				if (key == Keys.Escape)
					ChangeScreen(ScreenType.MENU, true);
			}

			// Key input
			if (key == Keys.KeyPadAdd || key == Keys.PageUp)
				Settings.CurrentMap++;
			if ((key == Keys.KeyPadSubtract || key == Keys.PageDown) && Settings.CurrentMap >= -1)
				Settings.CurrentMap--;

			if (key == Keys.RightAlt)
				Settings.PartyMode = !Settings.PartyMode;

			if (isAlt)
			{
				if (Type != GameType.EDITOR)
				{
					if (key == Keys.V)
						World.LocalPlayer.Health.HP = 0;

					if (key == Keys.B)
						World.LocalPlayer.Health.HP += 100;
				}

				if (key == Keys.N)
				{
					Statistics.Mana += 100;
					Statistics.Mana = Math.Clamp(Statistics.Mana, 0, Statistics.MaxMana);
				}

				if (key == Keys.M)
					Statistics.Money += 100;

				if (key == Keys.Comma)
					Settings.EnableInfoScreen = !Settings.EnableInfoScreen;

				if (key == Keys.Period)
				{
					World.ShroudLayer.RevealAll = true;
					WorldRenderer.CheckVisibility(Camera.LookAt, Camera.DefaultZoom);
				}

				if (key == Keys.X)
					SwitchEditor();
			}
		}

		public void CheckVictory()
		{
			switch (Mode)
			{
				// FIND_EXIT and TUTORIAL will meet conditions when entering the exit
				case GameMode.WAVES:
					if (waveController.Done())
						VictoryConditionsMet();

					break;
				case GameMode.KILL_ENEMIES:
					var actor = World.ActorLayer.NonNeutralActors.Find(a => a.Team != Actor.PlayerTeam && a.WorldPart != null && a.WorldPart.KillForVictory && !(a.Team == Actor.PlayerTeam || a.Team == Actor.NeutralTeam));

					if (actor == null)
						VictoryConditionsMet();
					break;
			}
		}

		public void VictoryConditionsMet()
		{
			State = GameState.VICTORY;
			script?.OnWin();
			Finish();

			Statistics.Level++;
			if (World.PlayerAlive && World.LocalPlayer.Health != null)
				Statistics.Health = World.LocalPlayer.Health.RelativeHP;

			ChangeScreen(ScreenType.VICTORY);
		}

		public void DefeatConditionsMet()
		{
			State = GameState.DEFEAT;
			script?.OnLose();
			Finish();

			ChangeScreen(ScreenType.DEFEAT);
		}

		public int CurrentWave()
		{
			return waveController != null ? waveController.CurrentWave() : 0;
		}

		public Actor FindValidTarget(CPos pos, int team)
		{
			const int range = 5120;

			if (KeyInput.IsKeyDown(Keys.LeftShift))
				return null;

			// Look for actors in range.
			var sectors = World.ActorLayer.GetSectors(pos, range);
			var currentRange = long.MaxValue;
			Actor validTarget = null;
			foreach (var sector in sectors)
			{
				foreach (var actor in sector.Actors)
				{
					if (actor.Team == team || actor.WorldPart == null || !actor.WorldPart.Targetable || !actor.WorldPart.InTargetBox(pos) || !VisibilitySolver.IsVisible(actor.Position))
					continue;

					var dist = (actor.Position - pos).SquaredFlatDist;
					if (dist < currentRange)
					{
						currentRange = dist;
						validTarget = actor;
					}
				}
			}

			return validTarget;
		}

		public void SwitchEditor()
		{
			Log.WriteDebug("Editor Switched: " + Editor);

			Editor = !Editor;

			Screen defaultScreen;
			if (Editor)
				defaultScreen = new EditorScreen(this);
			else if (Type == GameType.MENU || Type == GameType.MAINMENU)
				defaultScreen = null;
			else
				defaultScreen = new DefaultScreen(this);

			ScreenControl.NewDefaultScreen(defaultScreen);

			ChangeScreen(ScreenType.DEFAULT);
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

		public object[] GetScriptState(out string name)
		{
			name = string.Empty;

			if (script == null)
				return null;

			name = script.File;

			return script.GetState();
		}

		public void ChangeScreen(ScreenType screen, bool pause)
		{
			Pause(pause);
			ChangeScreen(screen);
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

			Log.WriteDebug("Current game disposed!");
			Log.DebugIndentation--;
		}
	}
}
