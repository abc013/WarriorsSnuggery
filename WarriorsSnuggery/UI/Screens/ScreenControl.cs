using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;

namespace WarriorsSnuggery.UI.Screens
{
	public class ScreenControl
	{
		readonly Dictionary<ScreenType, Screen> cachedScreens = new Dictionary<ScreenType, Screen>();
		readonly Game game;

		public Screen Focused { get; private set; }
		public ScreenType FocusedType { get; private set; } = ScreenType.EMPTY;

		public readonly ChatBox Chat;
		public bool ChatOpen { get; private set; }

		public ScreenControl(Game game)
		{
			this.game = game;

			Chat = new ChatBox(new CPos(0, 4096, 0));
		}

		public void InitScreen()
		{
			var defaultScreen = game.Type == GameType.EDITOR ? new EditorScreen(game) : (Screen)new DefaultScreen(game);
			cachedScreens.Add(ScreenType.DEFAULT, defaultScreen);

			ShowScreen(ScreenType.DEFAULT);
		}

		public void NewDefaultScreen(Screen screen)
		{
			cachedScreens[ScreenType.DEFAULT] = screen;
		}

		public void SetDecision(Action OnDecline, Action OnAgree, string text)
		{
			if (!cachedScreens.ContainsKey(ScreenType.DECISION))
				cachedScreens.Add(ScreenType.DECISION, new DecisionScreen(game));

			(cachedScreens[ScreenType.DECISION] as DecisionScreen).SetAction(OnDecline, OnAgree, text);
		}

		public void ShowScreen(ScreenType type)
		{
			if (FocusedType == type)
				return;

			if (type == ScreenType.EMPTY)
			{
				Focused.Hide();

				Focused = null;
				FocusedType = ScreenType.EMPTY;

				return;
			}

			Focused?.Hide();

			if (!cachedScreens.ContainsKey(type))
				createScreen(type);

			FocusedType = type;
			Focused = cachedScreens[type];
			Focused.Show();
		}

		void createScreen(ScreenType type)
		{
			var classType = Type.GetType("WarriorsSnuggery.UI.Screens." + type.ToString() + "Screen", true, true);

			cachedScreens.Add(type, (Screen)Activator.CreateInstance(classType, new object[] { game }));
		}

		public bool CursorOnUI()
		{
			return ChatOpen && Chat.MouseOnChat || Focused != null && Focused.CursorOnUI();
		}

		public void RefreshSaveGameScreens()
		{
			if (cachedScreens.ContainsKey(ScreenType.LOADGAME))
				((LoadGameScreen)cachedScreens[ScreenType.LOADGAME]).UpdateList();
			if (cachedScreens.ContainsKey(ScreenType.SAVEGAME))
				((SaveGameScreen)cachedScreens[ScreenType.SAVEGAME]).UpdateList();
		}

		public void UpdateSpells()
		{
			if (cachedScreens[ScreenType.DEFAULT] is DefaultScreen defaultScreen)
				defaultScreen.UpdateSpells();
		}

		public void UpdateActors()
		{
			if (cachedScreens[ScreenType.DEFAULT] is DefaultScreen defaultScreen)
				defaultScreen.UpdateActors();
		}

		public void UpdateWave(int wave, int final)
		{
			if (cachedScreens[ScreenType.DEFAULT] is DefaultScreen defaultScreen)
				defaultScreen.SetWave(wave, final);
		}

		public void Tick()
		{
			if (ChatOpen)
				Chat.Tick();
			else
				Focused?.Tick();
		}

		public void Render()
		{
			Focused?.Render();

			if (ChatOpen)
				Chat.Render();
		}

		public void DebugRender()
		{
			Focused?.DebugRender();

			if (ChatOpen)
				Chat.DebugRender();
		}

		public void KeyDown(Keys key, bool isControl, bool isShift, bool isAlt)
		{
			if (!ChatOpen)
			{
				Focused?.KeyDown(key, isControl, isShift, isAlt);

				if (key == Keys.Enter && isShift)
				{
					ChatOpen = true;
					Chat.OpenChat();
				}
			}
			else if (key == Keys.Escape || key == Keys.Enter && isShift)
			{
				ChatOpen = false;
				Chat.CloseChat();
			}
		}

		public void DisposeScreens()
		{
			cachedScreens.Clear();
			Focused = null;
			FocusedType = ScreenType.EMPTY;
		}
	}
}
