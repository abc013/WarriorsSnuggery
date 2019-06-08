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
				case GameType.TEST:
				case GameType.TUTORIAL:
				case GameType.NORMAL:
					defaultScreen = new DefaultScreen(Game);
					break;
			}
			screens.Add(ScreenType.DEFAULT, defaultScreen);
		}

		bool createScreen(ScreenType type)
		{
			if (screens.ContainsKey(type))
				return true;

			Screen screen = null;
			switch (type)
			{
				case ScreenType.MENU:
					screen = new MenuScreen(Game);
					break;
				case ScreenType.FAILURE:
					screen = new FailureScreen(Game);
					break;
				case ScreenType.WIN:
					screen = new WinScreen(Game);
					break;
				case ScreenType.KEYSETTINGS:
					screen = new KeyboardScreen(Game);
					break;
				case ScreenType.SETTINGS:
					screen = new SettingsScreen(Game);
					break;
				case ScreenType.PAUSED:
					screen = new PausedScreen(Game);
					break;
				case ScreenType.START:
					screen = new StartScreen(Game);
					break;
				case ScreenType.EDITORSELECTION:
					screen = new PieceScreen(Game);
					break;
				case ScreenType.SAVE:
					screen = new SaveGameScreen(Game);
					break;
				case ScreenType.LOAD:
					screen = new LoadGameScreen(Game);
					break;
				case ScreenType.TECHTREE:
					screen = new TechTreeScreen(Game);
					break;
				case ScreenType.NEW_STORY_GAME:
					screen = new NewGameScreen(Game);
					break;
				case ScreenType.NEW_CUSTOM_GAME:
					screen = new NewGameScreen(Game);
					break;
			}

			if (screen != null)
			{
				screens.Add(type, screen);

				return true;
			}

			return false;
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
				if (createScreen(screen))
					Focused = screens[screen];
				else
					Focused = null;
			}
			FocusedType = screen;
		}

		public void HideScreen()
		{
			Focused = null;
			FocusedType = ScreenType.NONE;
		}

		public bool CursorOnUI()
		{
			if (Focused == null)
				return false;

			return Focused.CursorOnUI();
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
