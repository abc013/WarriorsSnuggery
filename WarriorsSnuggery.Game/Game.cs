using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using WarriorsSnuggery.Audio;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Maps;
using WarriorsSnuggery.Objects;
using WarriorsSnuggery.Objects.Conditions;
using WarriorsSnuggery.Spells;
using WarriorsSnuggery.Scripting;
using WarriorsSnuggery.UI;
using WarriorsSnuggery.UI.Screens;

namespace WarriorsSnuggery
{
	public sealed class Game : ITick, IDisposable
	{
		public Random SharedRandom;

		public readonly ScreenControl ScreenControl;
		public readonly World World;

		public readonly GameStatistics OldStatistics;
		public readonly GameStatistics Statistics;
		public readonly MapType MapType;
		public readonly int Seed;

		public readonly MissionType MissionType;

		public bool IsMenu => MissionType == MissionType.MAIN_MENU || MissionType == MissionType.STORY_MENU || MissionType == MissionType.NORMAL_MENU || MissionType == MissionType.TUTORIAL_MENU;
		public bool IsCampaign => MissionType == MissionType.STORY || MissionType == MissionType.STORY_MENU || MissionType == MissionType.NORMAL || MissionType == MissionType.NORMAL_MENU;

		public MissionType MenuType
		{
			get
			{
				if (MissionType == MissionType.STORY || MissionType == MissionType.STORY_MENU)
					return MissionType.STORY_MENU;

				if (MissionType == MissionType.NORMAL || MissionType == MissionType.NORMAL_MENU)
					return MissionType.NORMAL_MENU;

				if (MissionType == MissionType.TUTORIAL || MissionType == MissionType.TUTORIAL_MENU)
					return MissionType.TUTORIAL_MENU;

				return MissionType;
			}
		}

		public MissionType CampaignType
		{
			get
			{
				if (MissionType == MissionType.STORY || MissionType == MissionType.STORY_MENU)
					return MissionType.STORY;

				if (MissionType == MissionType.NORMAL || MissionType == MissionType.NORMAL_MENU)
					return MissionType.NORMAL;

				if (MissionType == MissionType.TUTORIAL || MissionType == MissionType.TUTORIAL_MENU)
					return MissionType.TUTORIAL;

				return MissionType;
			}
		}

		public readonly InteractionMode InteractionMode;

		public readonly ObjectiveType ObjectiveType;

		public readonly SpellManager SpellManager;
		public readonly ConditionManager ConditionManager;

		readonly WaveController waveController;

		public int CurrentWave => waveController != null ? waveController.CurrentWave : 0;
		public int Waves => waveController != null ? waveController.Waves : 0;

		readonly MissionScriptBase script;

		readonly UITextLine infoText;

		int infoTextDuration;

		public uint LocalTick;
		public uint LocalRender;

		public bool Paused;
		public bool Editor;

		public bool WinConditionsMet;
		public bool Finished;

		MissionType nextLevelType;
		InteractionMode nextInteractionMode;
		bool nextLevel;

		bool counterStarted;
		int counterTick;

		public uint NextActorID => CurrentActorID++;
		public uint CurrentActorID;

		public uint NextWeaponID => CurrentWeaponID++;
		public uint CurrentWeaponID;

		public Game(GameStatistics statistics, MapType map, MissionType missionType, InteractionMode interactionMode, int seed = -1)
		{
			Log.WriteDebug("Loading new game...");
			Log.DebugIndentation++;

			Log.WriteDebug("MissionType: " + missionType);
			Log.WriteDebug("InteractionMode: " + interactionMode);

			MissionType = missionType;
			InteractionMode = interactionMode;

			MapType = map;

			// If seed negative, calculate it.
			Seed = seed < 0 ? statistics.Seed + statistics.Level : seed;
			SharedRandom = new Random(Seed);

			ObjectiveType = MapType.AvailableObjectives[SharedRandom.Next(map.AvailableObjectives.Length)];

			// In case of death, use this statistic.
			OldStatistics = statistics.Copy();
			// In case of success, use this statistic.
			Statistics = statistics;

			Editor = InteractionMode == InteractionMode.EDITOR;

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

			if (ObjectiveType == ObjectiveType.SURVIVE_WAVES)
				waveController = new WaveController(this);

			infoText = new UITextLine(FontManager.Pixel16);
		}

		public void AddInfoMessage(int duration, string text)
		{
			var corner = (int)(WindowInfo.UnitWidth / 2 * 1024);
			infoText.Position = new CPos(-corner + 512, -6144 - 256, 0);
			infoText.WriteText(text);
			infoTextDuration = duration;
		}

		public void Load()
		{
			var timer = Timer.Start();

			MasterRenderer.ResetRenderer(this);

			World.Load();

			ScreenControl.InitScreen();

			timer.StopAndWrite("Loading Game");

			if (World.LocalPlayer != null && World.LocalPlayer.Health != null && Statistics.Health > 0)
				World.LocalPlayer.Health.RelativeHP = Statistics.Health;

			WorldRenderer.CheckVisibilityAll();
			MasterRenderer.UpdateView();

			if (World.Map.Type.IsSave)
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
			if (nextLevel)
			{
				Log.WriteDebug("Instant level change initiated.");
				GameController.CreateNext(nextLevelType, nextInteractionMode);
				return;
			}

			LocalTick++;

			if (!Paused && !Finished)
			{
				if (counterStarted)
				{
					if (counterTick <= 0)
						finishCounter();
					else if (counterTick % Settings.UpdatesPerSecond == 0)
					{
						var seconds = counterTick / Settings.UpdatesPerSecond;
						AddInfoMessage(200, Color.Yellow + "Transfer in " + seconds + " second" + (seconds > 1 ? "s" : string.Empty));
					}

					counterTick--;
				}

				CheckVictory();

				// camera input
				if (!ScreenControl.CursorOnUI() && !(Camera.LockedToPlayer && World.PlayerAlive && !Editor))
				{
					var mouse = MouseInput.WindowPosition;

					var add = CPos.Zero;

					if (KeyInput.IsKeyDown(Settings.GetKey("CameraUp")) || (mouse.Y < 0 && mouse.Y < -WindowInfo.Height * 512 + 64 * Settings.EdgeScrolling))
						add = new CPos(add.X, add.Y - 1, 0);

					if (KeyInput.IsKeyDown(Settings.GetKey("CameraDown")) || (mouse.Y > 0 && mouse.Y > WindowInfo.Height * 512 - 64 * Settings.EdgeScrolling))
						add = new CPos(add.X, add.Y + 1, 0);

					if (KeyInput.IsKeyDown(Settings.GetKey("CameraRight")) || (mouse.X > 0 && mouse.X > WindowInfo.Width * 512 - 64 * Settings.EdgeScrolling))
						add = new CPos(add.X + 1, add.Y, 0);

					if (KeyInput.IsKeyDown(Settings.GetKey("CameraLeft")) || (mouse.X < 0 && mouse.X < -WindowInfo.Width * 512 + 64 * Settings.EdgeScrolling))
						add = new CPos(add.X - 1, add.Y, 0);

					if (add != CPos.Zero)
						Camera.Move(add);
				}

				// Zooming
				if (!ScreenControl.CursorOnUI() && !Editor && InteractionMode != InteractionMode.EDITOR)
				{
					if (KeyInput.IsKeyDown(Keys.LeftControl) && MouseInput.IsRightDown)
						Camera.Zoom(Settings.ScrollSpeed / 20 * (4 - (Camera.CurrentZoom - Camera.DefaultZoom) / 2));
					else
						Camera.Zoom(Settings.ScrollSpeed / 20 * (-(Camera.CurrentZoom - Camera.DefaultZoom) / 2));
				}

				// party mode
				if (Settings.PartyMode)
				{
					var sin1 = MathF.Sin(LocalTick / 8f) / 2 + 0.8f;
					var sin2 = MathF.Sin(LocalTick / 8f + Angle.MaxRange / 3) / 2 + 0.8f;
					var sin3 = MathF.Sin(LocalTick / 8f + 2 * Angle.MaxRange / 3) / 2 + 0.8f;

					WorldRenderer.Ambient = new Color(sin1, sin2, sin3);
				}

				SpellManager.Tick();
				ConditionManager.Tick();
				World.Tick();

				script?.Tick();

				if (ObjectiveType == ObjectiveType.SURVIVE_WAVES)
					waveController.Tick();
			}

			if (infoTextDuration-- < 100)
				infoText.Position -= new CPos(64, 0, 0);

			if (Window.GlobalTick == 0 && Settings.FirstStarted)
				ShowScreen(ScreenType.START, true);

			ScreenControl.Tick();
		}

		public void KeyDown(Keys key, bool isControl, bool isShift, bool isAlt)
		{
			var screenTypeBefore = ScreenControl.FocusedType;

			if (key == Settings.GetKey("Pause") && !isControl && ScreenControl.FocusedType != ScreenType.PAUSED && ScreenControl.FocusedType != ScreenType.DEFEAT)
			{
				if (!(ScreenControl.ChatOpen && ScreenControl.CursorOnUI()))
				{
					ShowScreen(ScreenType.PAUSED, true);
					return;
				}
			}

			ScreenControl.KeyDown(key, isControl, isShift, isAlt);

			if (Paused || Finished || ScreenControl.ChatOpen)
				return;

			// screen control
			if (ScreenControl.FocusedType != ScreenType.DEFEAT && screenTypeBefore != ScreenType.MENU)
			{
				if (key == Keys.Escape)
					ShowScreen(ScreenType.MENU, true);
			}

			// Player lock
			if (key == Settings.GetKey("CameraLock"))
				Camera.LockedToPlayer = !Camera.LockedToPlayer;

			// Key input
			if (key == Keys.KeyPadAdd || key == Keys.PageUp)
				Settings.CurrentMap++;
			if ((key == Keys.KeyPadSubtract || key == Keys.PageDown) && Settings.CurrentMap >= -1)
				Settings.CurrentMap--;

			if (key == Keys.RightAlt)
				Settings.PartyMode = !Settings.PartyMode;

			// Cheats
			if (Settings.EnableCheats && isAlt)
			{
				if (InteractionMode != InteractionMode.EDITOR)
				{
					if (key == Keys.C)
						VictoryConditionsMet(true);

					if (key == Keys.V)
						DefeatConditionsMet();

					if (key == Keys.B)
						World.LocalPlayer.Health.HP += 100;

					if (key == Keys.N)
					{
						Statistics.Mana += 100;
						Statistics.Mana = Math.Clamp(Statistics.Mana, 0, Statistics.MaxMana);
					}

					if (key == Keys.M)
						Statistics.Money += 100;

					if (key == Keys.Period)
					{
						World.ShroudLayer.RevealAll = !World.ShroudLayer.RevealAll;
						WorldRenderer.CheckVisibility(Camera.LookAt, Camera.DefaultZoom);
					}

					if (key == Keys.X)
						SwitchEditor();
				}

				if (key == Keys.Comma)
					Settings.EnableInfoScreen = !Settings.EnableInfoScreen;
			}
		}

		public void CheckVictory()
		{
			switch (ObjectiveType)
			{
				// FIND_EXIT will meet conditions when entering the exit
				case ObjectiveType.SURVIVE_WAVES:
					if (waveController.Done)
						VictoryConditionsMet();

					break;
				case ObjectiveType.KILL_ENEMIES:
					var actor = World.ActorLayer.NonNeutralActors.Find(a => a.Team != Actor.PlayerTeam && a.WorldPart != null && a.WorldPart.KillForVictory && !(a.Team == Actor.PlayerTeam || a.Team == Actor.NeutralTeam));

					if (actor == null)
						VictoryConditionsMet();
					break;
			}
		}

		public void VictoryConditionsMet(bool instantFinish = false)
		{
			if (WinConditionsMet)
				return;

			WinConditionsMet = true;

			script?.OnWin();
			Statistics.Level++;
			if (World.PlayerAlive && World.LocalPlayer.Health != null)
				Statistics.Health = World.LocalPlayer.Health.RelativeHP;

			if (instantFinish)
				finishCounter();
			else
				startTransferCounter();
		}

		void startTransferCounter()
		{
			counterStarted = true;
			// Give 10 seconds
			counterTick = Settings.UpdatesPerSecond * 10;
		}

		void finishCounter()
		{
			Finish();

			ShowScreen(ScreenType.VICTORY);
		}

		public void DefeatConditionsMet()
		{
			script?.OnLose();
			Finish();

			ShowScreen(ScreenType.DEFEAT);
		}

		public void SwitchEditor()
		{
			Log.WriteDebug("Editor Switched: " + Editor);

			Editor = !Editor;

			// Automatically switches to the correct one
			ShowScreen(ScreenType.DEFAULT);
		}

		// Instant travel to next level
		public void ChangeLevelAfterTick(MissionType newType, InteractionMode newMode = InteractionMode.INGAME)
		{
			nextLevelType = newType;
			nextInteractionMode = newMode;
			nextLevel = true;
		}

		public void Finish()
		{
			script?.OnFinish();
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

		public void ShowScreen(ScreenType screen, bool pause)
		{
			Pause(pause);
			ShowScreen(screen);
		}

		public void ShowScreen(ScreenType screen)
		{
			if (InteractionMode != InteractionMode.EDITOR && Editor && screen == ScreenType.DEFAULT)
				ScreenControl.ShowScreen(ScreenType.EDITOR);
			else
				ScreenControl.ShowScreen(screen);
		}

		public void ShowDecisionScreen(Action onDecline, Action onAgree, string text)
		{
			ScreenControl.SetDecision(onDecline, onAgree, text);
			ScreenControl.ShowScreen(ScreenType.DECISION);
		}

		public void RefreshSaveGameScreens()
		{
			ScreenControl.RefreshSaveGameScreens();
		}

		public void RenderDebug()
		{
			if (infoTextDuration > 0) infoText.Render();
		}

		public void Dispose()
		{
			if (!Finished)
				throw new Exception("Game has not been finished before dispose call.");

			World.Dispose();

			ScreenControl.DisposeScreens();

			AudioController.StopAll(true);

			Log.WriteDebug("Current game disposed!");
			Log.DebugIndentation--;
		}
	}
}
