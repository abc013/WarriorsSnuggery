using System;
using System.Collections.Generic;

namespace WarriorsSnuggery.UI
{
	public class ScreenControl
	{
		public readonly Game Game;

		public Screen Focused;
		public ScreenType FocusedType;

		readonly Dictionary<ScreenType, Screen> screens = new Dictionary<ScreenType, Screen>(); 

		public ScreenControl(Game game)
		{
			Game = game;
		}

		public void Load()
		{
			Screen defaultScreen = null;
			switch (Game.Type)
			{
				case GameType.EDITOR:
					defaultScreen = new EditorScreen(Game);
					break;
				case GameType.TUTORIAL:
				case GameType.NORMAL:
					defaultScreen = new DefaultScreen(Game);
					break;
			}
			screens.Add(ScreenType.DEFAULT, defaultScreen);

			screens.Add(ScreenType.MENU, new MenuScreen(Game));
			screens.Add(ScreenType.DEATH, new DeathScreen(Game));

			screens.Add(ScreenType.KEYSETTINGS, new KeyboardScreen(Game));
			screens.Add(ScreenType.SETTINGS, new SettingsScreen(Game));

			screens.Add(ScreenType.PAUSED, new PausedScreen(Game));
			screens.Add(ScreenType.START, new StartScreen(Game));

			screens.Add(ScreenType.EDITORSELECTION, new PieceScreen(Game));

			screens.Add(ScreenType.SAVE, new SaveGameScreen(Game));
			screens.Add(ScreenType.LOAD, new LoadGameScreen(Game));

			screens.Add(ScreenType.TECHTREE, new TechTreeScreen(Game));
		}

		public void NewDefaultScreen(Screen screen)
		{
			if (screens[ScreenType.DEFAULT] != null)
				screens[ScreenType.DEFAULT].Dispose();
			screens[ScreenType.DEFAULT] = screen;
		}

		public void ShowScreen(ScreenType screen)
		{
			if (screens.ContainsKey(screen))
			{
				Focused = screens[screen];
			}
			else
			{
				Focused = null;
			}
			FocusedType = screen;
		}

		public void HideScreen()
		{
			Focused = null;
			FocusedType = ScreenType.NONE;
		}

		public void Render()
		{
			if (Focused == null)
				return;

			Focused.Render();
		}

		public void Tick()
		{
			if (Focused == null)
				return;

			Focused.Tick();
		}

		public void DisposeScreens()
		{
			foreach (var screen in screens.Values)
				screen?.Dispose();
			screens.Clear();
			Focused = null;
			FocusedType = ScreenType.NONE;
		}

		public void RefreshSaveGameScreens()
		{
			((LoadGameScreen)screens[ScreenType.LOAD]).UpdateList();
			((SaveGameScreen)screens[ScreenType.SAVE]).UpdateList();
		}
	}
}
