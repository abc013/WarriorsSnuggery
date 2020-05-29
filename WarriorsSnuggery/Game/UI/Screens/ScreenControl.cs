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
				case ScreenType.DEFEAT:
					screen = new FailureScreen(Game);
					break;
				case ScreenType.VICTORY:
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
				case ScreenType.SPELL_SHOP:
					screen = new SpellShopScreen(Game);
					break;
				case ScreenType.ACTOR_SHOP:
					screen = new ActorShopScreen(Game);
					break;
				case ScreenType.NEW_STORY_GAME:
					screen = new NewGameScreen(Game);
					break;
				case ScreenType.NEW_CUSTOM_GAME:
					screen = new NewGameScreen(Game);
					break;
				case ScreenType.DECISION:
					screen = new ConfirmationScreen();
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
			screens[ScreenType.DEFAULT] = screen;
		}

		public void UpdateSpells()
		{
			if (screens[ScreenType.DEFAULT] is DefaultScreen defaultScreen)
				defaultScreen.UpdateSpells();
		}

		public void UpdateActors()
		{
			if (screens[ScreenType.DEFAULT] is DefaultScreen defaultScreen)
				defaultScreen.UpdateActors();
		}

		public void SetDecision(Action OnDecline, Action OnAgree, string text)
		{
			if (!screens.ContainsKey(ScreenType.DECISION))
				createScreen(ScreenType.DECISION);

			// Should not crash as the decisionScreen should always be a decisionScreen
			(screens[ScreenType.DECISION] as ConfirmationScreen).SetAction(OnDecline, OnAgree, text);
		}

		public void ShowScreen(ScreenType screen)
		{
			if (screens.ContainsKey(screen))
			{
				setFocused(screens[screen]);
			}
			else
			{
				if (createScreen(screen))
					setFocused(screens[screen]);
				else
					setFocused(null);
			}
			FocusedType = screen;
		}

		void setFocused(Screen screen)
		{
			Focused?.Hide();
			Focused = screen;
			Focused?.Show();
		}

		public void HideScreen()
		{
			Focused?.Hide();
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
			screens.Clear();
			Focused = null;
			FocusedType = ScreenType.NONE;
		}

		public void RefreshSaveGameScreens()
		{
			if (screens.ContainsKey(ScreenType.LOAD))
				((LoadGameScreen)screens[ScreenType.LOAD]).UpdateList();
			if (screens.ContainsKey(ScreenType.SAVE))
				((SaveGameScreen)screens[ScreenType.SAVE]).UpdateList();
		}
	}
}
