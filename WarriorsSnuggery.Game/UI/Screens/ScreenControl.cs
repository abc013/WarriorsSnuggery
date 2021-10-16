using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.UI.Objects;

namespace WarriorsSnuggery.UI.Screens
{
	public class ScreenControl
	{
		readonly Dictionary<ScreenType, Screen> cachedScreens = new Dictionary<ScreenType, Screen>();
		readonly Game game;

		public Screen Focused { get; private set; }
		public ScreenType FocusedType { get; private set; } = ScreenType.EMPTY;

		public readonly ChatBox Chat;
		public bool ChatOpen => Chat.Visible;

		public readonly MessageBox Message;
		public bool MessageDisplayed => Message.Visible;

		public readonly InfoScreen Screen;
		public static bool InfoScreenOpen => Settings.EnableInfoScreen;

		public readonly InfoText Text;

		readonly GameTransitionFader fader = new GameTransitionFader();

		public ScreenControl(Game game)
		{
			this.game = game;

			Chat = new ChatBox(new CPos(0, 4096, 0));
			Message = new MessageBox(new CPos(0, 4096, 0));
			Screen = new InfoScreen();
			Text = new InfoText();
		}

		public void Load()
		{
			if (game.InteractionMode == InteractionMode.NONE)
			{
				cachedScreens.Add(ScreenType.DEFAULT, null);
				return;
			}

			var defaultScreen = game.InteractionMode == InteractionMode.EDITOR ? new EditorScreen(game) : (Screen)new DefaultScreen(game);
			cachedScreens.Add(ScreenType.DEFAULT, defaultScreen);

			ShowScreen(ScreenType.DEFAULT);
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
			Focused?.Show();
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

		public void ShowArrow()
		{
			if (cachedScreens[ScreenType.DEFAULT] is DefaultScreen defaultScreen)
				defaultScreen.ShowArrow();
		}

		public void HideArrow()
		{
			if (cachedScreens[ScreenType.DEFAULT] is DefaultScreen defaultScreen)
				defaultScreen.HideArrow();
		}

		public void OpenMessage(Message message) => Message.OpenMessage(message);
		public void AddInfoMessage(int duration, string message) => Text.SetMessage(duration, message);

		public void FadeIn() => fader.FadeIn();
		public void FadeOut() => fader.FadeOut();
		public bool FadeDone => fader.FadeDone;

		public void Tick()
		{
			if (!ChatOpen)
				Focused?.Tick();

			Chat.Tick();
			Message.Tick();

			fader.Tick();
			Screen.Tick();

			Text.Tick();
		}

		public void Render()
		{
			Focused?.Render();

			Chat.Render();
			Message.Render();

			fader.Render();
			Screen.Render();

			Text.Render();
		}

		public void DebugRender()
		{
			Focused?.DebugRender();

			if (ChatOpen)
				Chat.DebugRender();

			if (MessageDisplayed)
				Message.DebugRender();

			Text.DebugRender();
		}

		public void KeyDown(Keys key, bool isControl, bool isShift, bool isAlt)
		{
			if (!ChatOpen)
			{
				Focused?.KeyDown(key, isControl, isShift, isAlt);

				if (key == Keys.Enter && isShift)
					Chat.OpenChat();
			}
			else
			{
				Chat.KeyDown(key, isControl, isShift, isAlt);

				if (key == Keys.Escape || key == Keys.Enter && isShift)
					Chat.CloseChat();
			}
		}

		public void DisposeScreens()
		{
			cachedScreens.Clear();
			Focused = null;
			FocusedType = ScreenType.EMPTY;
		}

		class GameTransitionFader
		{
			public bool FadeDone => fadeOut ? maxFade == fade : 0 == fade;
			bool fadeOut;

			const byte maxFade = 15;
			byte fade = maxFade;

			public void Tick()
			{
				if (!FadeDone)
				{
					if (fadeOut)
						fade++;
					else
						fade--;
				}
			}

			public void Render()
			{
				var darkness = (byte)((256 / maxFade) * fade);
				if (darkness > 0)
					ColorManager.DrawFullscreenRect(new Color(0, 0, 0, darkness));
			}

			public void FadeIn() => fadeOut = false;
			public void FadeOut() => fadeOut = true;
		}
	}
}
