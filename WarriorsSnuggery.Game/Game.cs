using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using WarriorsSnuggery.Audio;
using WarriorsSnuggery.Audio.Music;
using WarriorsSnuggery.Conditions;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Maps;
using WarriorsSnuggery.Spells;
using WarriorsSnuggery.Scripting;
using WarriorsSnuggery.UI.Screens;
using WarriorsSnuggery.Objects.Actors;
using WarriorsSnuggery.Objectives;

namespace WarriorsSnuggery
{
	public sealed class Game : ITick, IDisposable
	{
		public readonly Random SharedRandom;

		public readonly ScreenControl ScreenControl;
		public readonly World World;

		public readonly GameSave Save;
		public Player Player => Save.Player;
		public SpellCasterManager SpellManager => Save.Player.SpellCasters;
		public readonly GameSave OriginalSave;

		public readonly MapType MapType;
		public readonly int Seed;

		public readonly MissionType MissionType;

		public readonly InteractionMode InteractionMode;

		public readonly ObjectiveType ObjectiveType;

		public readonly ConditionManager ConditionManager;

		public readonly ObjectiveController ObjectiveController;

		public readonly MissionScriptBase Script;

		public uint LocalTick { get; private set; }

		public bool Paused { get; private set; }
		public bool Editor { get; private set; }

		public bool WinConditionsMet { get; private set; }
		public bool Finished { get; private set; }

		MissionType nextLevelType;
		InteractionMode nextInteractionMode;
		bool nextLevelIncrease;
		bool nextLevel;

		bool counterStarted;
		int counterTick;

		public uint NextActorID => CurrentActorID++;
		public uint CurrentActorID;

		public uint NextWeaponID => CurrentWeaponID++;
		public uint CurrentWeaponID;

		public Game(GameSave save, MapType map, MissionType missionType, InteractionMode interactionMode, int seed = -1)
		{
			Log.Debug($"Loading new game (MissionType: '{missionType}', InteractionMode: '{interactionMode}').");

			MissionType = missionType;
			InteractionMode = interactionMode;

			MapType = map;

			// If seed negative, calculate it.
			Seed = seed < 0 ? save.Seed + save.Level : seed;
			SharedRandom = new Random(Seed);

			ObjectiveType = MapType.AvailableObjectives[SharedRandom.Next(map.AvailableObjectives.Length)];

			Save = save;
			OriginalSave = save.Clone();

			Editor = InteractionMode == InteractionMode.EDITOR;

			ConditionManager = new ConditionManager(this);

			ScreenControl = new ScreenControl(this);

			World = new World(this, Seed, Save);

			if (map.MissionScript != null && !Program.DisableScripts)
			{
				var scriptLoader = new MissionScriptLoader(map.MissionScript);
				Script = scriptLoader.Start(this);
			}
			else
				Log.Debug(Program.DisableScripts ? "Mission scripts are disabled." : "No mission script existing.");

			ObjectiveController = ObjectiveController.GetController(this, ObjectiveType);
		}

		public void Load()
		{
			var timer = Timer.StartNew();

			MasterRenderer.PauseSequences = false;
			WorldRenderer.Reset(this);
			UIRenderer.Reset(this);
			Camera.Reset();
			MusicController.ResetIntense();

			World.Load();

			if (MapType.Music != null)
				MusicController.LoopSong(MapType.Music, MapType.IntenseMusic);
			else
				MusicController.LoopAllSongs();

			ScreenControl.Load();
			ScreenControl.FadeIn();

			if (MapType.IsSave)
				Script?.Load(Save.ScriptState);
			else
				Script?.OnStart();
			
			if (MapType.IsSave)
				ObjectiveController.Load(Save.ObjectiveController);

			timer.StopAndWrite("Loading Game");
			Log.Debug("Loading successful!");
		}

		public void Pause(bool paused)
		{
			Paused = paused;
			AudioController.PauseAll(paused, true);
			MasterRenderer.PauseSequences = Paused;
			Camera.Locked = Paused;
			Log.Debug((paused ? "P" : "Unp") + "aused game.");
		}

		public void Tick()
		{
			if (Finished && GameController.NextGamePrepared && ScreenControl.FadeDone)
			{
				GameController.LoadNext();
				return;
			}

			if (nextLevel)
			{
				Log.Debug("Instant game change requested. Executing now.");

				Save.Update(this, nextLevelIncrease);
				GameController.CreateNext(nextLevelType, nextInteractionMode);

				nextLevel = false;
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
						AddInfoMessage(200, $"{Color.Yellow}Transfer in {seconds} second{(seconds > 1 ? "s" : "")}");
					}

					counterTick--;
				}

				// screenshaker
				if (Screenshaker.ShakeStrength > 0)
				{
					Camera.Update();
					Screenshaker.DecreaseShake();
				}

				// camera input
				if (!ScreenControl.CursorOnUI())
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
					if (KeyInput.IsKeyDown(Keys.Tab))
						Camera.Zoom(Settings.ScrollSpeed * ((Camera.MaxZoom - Camera.CurrentZoom) / 40));
					else
						Camera.Zoom(Settings.ScrollSpeed * ((Camera.DefaultZoom - Camera.CurrentZoom) / 40));
				}

				SpellManager.Tick();
				ConditionManager.Tick();
				World.Tick();

				Script?.Tick();
				if (!Editor)
					ObjectiveController?.Tick();
			}

			if (Window.GlobalTick == 0 && Settings.FirstStarted)
				ShowScreen(ScreenType.START, true);

			ScreenControl.Tick();
		}

		public void KeyDown(Keys key, bool isControl, bool isShift, bool isAlt)
		{
			// Info screen
			if (key == Keys.Comma)
				Settings.EnableInfoScreen = !Settings.EnableInfoScreen;

			// Debug map cycling
			if (key == Keys.KeyPadAdd || key == Keys.PageUp)
				Settings.CurrentMap++;
			if ((key == Keys.KeyPadSubtract || key == Keys.PageDown) && Settings.CurrentMap >= -1)
				Settings.CurrentMap--;

			// Party mode
			if (key == Keys.RightAlt)
				Settings.PartyMode = !Settings.PartyMode;

			var screenTypeBefore = ScreenControl.FocusedType;
			ScreenControl.KeyDown(key, isControl, isShift, isAlt);

			if (Paused || Finished)
				return;

			if (ScreenControl.CursorOnUI() || ScreenControl.ChatOpen)
				return;

			if (ScreenControl.FocusedType != ScreenType.DEFEAT)
			{
				if (screenTypeBefore != ScreenType.PAUSED && key == Settings.GetKey("Pause") && !isControl)
					ShowScreen(ScreenType.PAUSED, true);

				if (screenTypeBefore != ScreenType.MENU && key == Keys.Escape)
					ShowScreen(ScreenType.MENU, true);
			}

			// Player lock
			if (key == Settings.GetKey("CameraLock"))
				Settings.LockCameraToPlayer = !Settings.LockCameraToPlayer;

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
						Player.Mana += 100;

					if (key == Keys.M)
						Player.Money += 100;

					if (key == Keys.Period)
						World.ShroudLayer.RevealAll = !World.ShroudLayer.RevealAll;

					if (key == Keys.X)
						SwitchEditor();

					if (key == Keys.R)
					{
						var name = Save.Name;
						GameSaveManager.SaveOnNewName(Save, "temp", this);
						Save.SetName(name);
						GameSaveManager.Reload();
						var save = GameSaveManager.Saves.Find(g => g.SaveName == "temp");
						GameController.CreateFromSave(save.Clone());
					}
				}
			}
		}

		public void VictoryConditionsMet(bool instantFinish = false)
		{
			if (WinConditionsMet)
				return;

			WinConditionsMet = true;

			Script?.OnWin();
			Save.Update(this, true);

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

			MusicController.FadeIntenseOut();
		}

		void finishCounter()
		{
			Finish(false);

			ShowScreen(ScreenType.VICTORY);
		}

		public void DefeatConditionsMet()
		{
			Script?.OnLose();
			Finish(false);

			ShowScreen(ScreenType.DEFEAT);
		}

		public void SwitchEditor()
		{
			Editor = !Editor;
			Log.Debug($"Editor {(Editor ? "En" : "Dis")}abled.");

			// Automatically switches to the correct one
			ShowScreen(ScreenType.DEFAULT);
		}

		// Instant travel to next level
		public void ChangeLevelAfterTick(MissionType newType, InteractionMode newMode = InteractionMode.INGAME, bool increaseLevel = false)
		{
			nextLevelType = newType;
			nextInteractionMode = newMode;
			nextLevelIncrease = increaseLevel;
			nextLevel = true;
		}

		public void Finish(bool fade = true)
		{
			Script?.OnFinish();
			Finished = true;
			Pause(true);

			if (fade)
			{
				ScreenControl.FadeOut();
				MusicController.FadeIntenseOut();
			}
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

		public void AddInfoMessage(int duration, string text)
		{
			ScreenControl.ShowInformation(duration, text);
		}

		public void Dispose()
		{
			if (!Finished)
				throw new Exception("Game has not been finished before dispose call.");

			AudioController.StopAll(true);

			Log.Debug("Current game disposed!");
		}
	}
}
