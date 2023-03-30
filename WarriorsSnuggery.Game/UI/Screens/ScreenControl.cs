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

		readonly ChatBox chat = new ChatBox(new UIPos(0, 4096));
		readonly MessageBox message = new MessageBox(new UIPos(0, 4096));
		readonly DebugBox debugBox = new DebugBox();
		readonly InfoText infoText = new InfoText();

		readonly GameTransitionFader fader = new GameTransitionFader();

		public ScreenControl(Game game)
		{
			this.game = game;
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
				createScreen(ScreenType.DECISION);

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
			return chat.Visible && UIUtils.ContainsMouse(chat)
				|| message.Visible && UIUtils.ContainsMouse(message)
				|| Focused != null && Focused.CursorOnUI();
		}

		public void ReloadScreenCache()
		{
			var type = FocusedType;

			ShowScreen(ScreenType.EMPTY);
			cachedScreens.Clear();
			ShowScreen(type);
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

		public void ShowMessage(Message message) => this.message.OpenMessage(message);
		public void ShowInformation(int duration, string message) => infoText.ShowInformation(duration, message);
		public bool ChatOpen => chat.Visible;

		public void FadeIn() => fader.FadeIn();
		public void FadeOut() => fader.FadeOut();
		public bool FadeDone => fader.FadeDone;

		public void Tick()
		{
			if (!ChatOpen)
				Focused?.Tick();

			chat.Tick();
			message.Tick();

			fader.Tick();
			debugBox.Tick();

			infoText.Tick();
		}

		public void Render()
		{
			Focused?.Render();

			chat.Render();
			message.Render();

			fader.Render();
			debugBox.Render();

			infoText.Render();
		}

		public void DebugRender()
		{
			Focused?.DebugRender();

			if (ChatOpen)
				chat.DebugRender();

			infoText.DebugRender();
		}

		public void KeyDown(Keys key, bool isControl, bool isShift, bool isAlt)
		{
			if (!ChatOpen)
			{
				Focused?.KeyDown(key, isControl, isShift, isAlt);

				if (key == Keys.Enter && isShift)
					chat.OpenChat();
			}
			else
			{
				chat.KeyDown(key, isControl, isShift, isAlt);

				if (key == Keys.Escape || key == Keys.Enter && isShift)
					chat.CloseChat();
			}
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
				var darkness = (byte)((255 / maxFade) * fade);
				if (darkness > 0)
					ColorManager.DrawFullscreenRect(Color.Black.WithAlpha(darkness / 255f));
			}

			public void FadeIn() => fadeOut = false;
			public void FadeOut() => fadeOut = true;
		}
	}
}
