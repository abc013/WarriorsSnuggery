using System;
using System.Linq;
using System.Collections.Generic;
using WarriorsSnuggery.Objects;
using WarriorsSnuggery.UI;
using WarriorsSnuggery.Maps;

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
		FIND_EXIT
	}

	public class Game : ITick, IDisposable
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

		public List<GameObject> UI = new List<GameObject>();

		public readonly Window Window;

		public readonly GameStatistics Statistics;
		public readonly MapType MapType;
		public readonly GameType Type;
		public readonly int Seed;

		readonly GameObject MousePosition, CenterPosition;
		readonly Text tick;
		readonly Text render;
		readonly Text memory;
		readonly Text version;

		readonly Text infoText;
		int infoTextDuration;

		readonly Button camToPlayer;

		public Game(GameStatistics statistics, MapType map, int seed = -1)
		{
			Window = Window.Current;

			if (seed < 0)
				seed = SharedRandom.Next();
			Seed = seed;
			MapType = map;
			Statistics = statistics;

			Type = MapType.DefaultType;
			if (Type == GameType.EDITOR)
				Editor = true;

			ScreenControl = new ScreenControl(this);

			World = new World(this, seed, Statistics.Level, Statistics.Difficulty);

			MasterRenderer.ResetRenderer(this);

			var corner = (int) (WindowInfo.UnitWidth / 2 * 1024);
			version = new Text(new CPos(corner, 6192,0), IFont.Pixel16, Text.OffsetType.RIGHT);
			version.SetColor(Color.Yellow);
			version.SetText(Settings.Version);

			memory = new Text(new CPos(corner, 6692,0), IFont.Pixel16, Text.OffsetType.RIGHT);
			tick = new Text(new CPos(corner, 7692,0), IFont.Pixel16, Text.OffsetType.RIGHT);
			render = new Text(new CPos(corner, 7192,0), IFont.Pixel16, Text.OffsetType.RIGHT);

			camToPlayer = ButtonCreator.Create("wooden", new CPos(0, (int) (WindowInfo.UnitHeight * 512) - 512, 0), "Cam to Player", () => { Camera.LockedToPlayer = !Camera.LockedToPlayer; });
			infoText = new Text(new CPos(-corner + 1024, 7192, 0), IFont.Pixel16);

			if (Settings.EnableDebug)
			{
				var height = new CPos(0, 0, 1000);
				MousePosition = new ColoredRect(height, Color.Red, 0.1f);
				CenterPosition = new ColoredRect(height, Color.Blue, 0.1f);
				UI.Add(MousePosition);
				UI.Add(CenterPosition);
			}
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

			foreach(var obj in UI)
				obj.Tick();

			if (!Paused)
			{
				camToPlayer.Tick();
				if (ScreenControl.FocusedType != ScreenType.DEATH)
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
				}

				if (Settings.PartyMode)
				{
					var sin1 = (float) Math.Sin(LocalTick / 8f);
					var sin2 = (float) Math.Sin(LocalTick / 8f + 2 * Math.PI / 3);
					var sin3 = (float) Math.Sin(LocalTick / 8f + 4 * Math.PI / 3);

					if (sin1 < 0) sin1 = 0;
					if (sin2 < 0) sin2 = 0;
					if (sin3 < 0) sin3 = 0;

					WorldRenderer.Ambient = new Color(sin1, sin2, sin3);
				}

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
					add = new CPos(add.X + (int) 1, add.Y, 0);
				}
				if (KeyInput.IsKeyDown(Settings.Key("CameraLeft"), 0) || (mouse.X < 0 && mouse.X < -Camera.DefaultZoom * WindowInfo.Ratio * 512 + 64 * Settings.EdgeScrolling))
				{
					add = new CPos(add.X - (int) 1, add.Y, 0);
				}

				if (add != CPos.Zero)
				{
					Camera.Move(add);
				}

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
					}
					if (KeyInput.IsKeyDown("b", 10))
					{
						World.LocalPlayer.Health.HP += 100;
					}
					if (KeyInput.IsKeyDown("v", 10))
					{
						Statistics.Money += 100;
					}
				}

				if (MouseInput.isRightDown && Type != GameType.EDITOR)
				{
					Camera.Zoom(Settings.ScrollSpeed / 5 * (4 - (Camera.CurrentZoom - Camera.DefaultZoom) / 2));
				}
				else
				{
					Camera.Zoom(Settings.ScrollSpeed / 5 * (-(Camera.CurrentZoom - Camera.DefaultZoom) / 2));
				}
				World.Tick();
			}
			
			//memory.SetText("Memory " + (int) (System.Diagnostics.Process.GetCurrentProcess().PrivateMemorySize64 / 1024f) + " KB");
			memory.SetText("Public Memory " + (int)(GC.GetTotalMemory(false) / 1024f) + " KB");
			tick.SetColor(Window.Current.TPS < 50 ? Color.Red : Color.White);
			tick.SetText("Tick " + LocalTick + " @ " + Window.Current.TPS);
			
			tick.SetColor(Window.Current.FPS < Settings.FrameLimiter - 10 ? Color.Red : Color.White);
			render.SetText("Render " + LocalTick + " @ " + Window.Current.FPS);

			if (infoTextDuration-- < 90)
			{
				infoText.Position -= new CPos(48, 0, 0);
			}

			ScreenControl.Tick();

			if (Settings.EnableDebug)
				MousePosition.Position = MouseInput.WindowPosition;

			watch.Stop();
			if (LocalTick % 4 == 0)
				Log.WritePerformance(watch.ElapsedMilliseconds, " Tick " + LocalTick);

			UI = UI.OrderBy(o => o.GraphicPosition.Z).ToList();
			UI.RemoveAll(o => o.Disposed);

			if (ScreenControl.FocusedType == ScreenType.START)
				Pause(true);
		}

		public void ChangeScreen(ScreenType screen)
		{
			ScreenControl.ShowScreen(screen);

			switch (screen)
			{
				case ScreenType.DEATH:
					Pause(true);
					ScreenControl.NewDefaultScreen(ScreenControl.Focused);
					break;
			}
		}

		public void RenderDebug()
		{
			version.Render();
			memory.Render();
			tick.Render();
			render.Render();

			if (infoTextDuration > 0) infoText.Render();

			if (!Paused) camToPlayer.Render();
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

			if(Settings.DeveloperMode)
			{
				CenterPosition.Dispose();
				MousePosition.Dispose();
			}

			version.Dispose();
			memory.Dispose();
			tick.Dispose();
			render.Dispose();

			infoText.Dispose();

			camToPlayer.Dispose();

			WorldRenderer.ClearRenderLists();
			UIRenderer.ClearRenderLists();
			UI.ForEach((o) => o.Dispose());
			UI.Clear();

			GC.Collect();
		}

		public void RefreshSaveGameScreens()
		{
			ScreenControl.RefreshSaveGameScreens();
		}
	}
}
