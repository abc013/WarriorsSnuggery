using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using WarriorsSnuggery.Audio;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;
using WarriorsSnuggery.UI.Objects;

namespace WarriorsSnuggery.UI.Screens
{
	public class SettingsScreen : Screen
	{
		readonly Game game;

		readonly CheckBox fullscreenCheck, vSyncCheck, developerModeCheck, pixelingCheck, textshadowCheck;
		readonly TextBox widthWrite, heightWrite, frameLimiterWrite;
		readonly SliderBar panningSlider, edgePanningSlider, masterVolumeSlider, effectVolumeSlider, musicVolumeSlider;

		public bool Visible { get; private set; }

		public SettingsScreen(Game game) : base("Settings")
		{
			this.game = game;
			Title.Position = new CPos(0, -4096, 0);

			// Window
			var fullscreen = new UITextLine(FontManager.Pixel16) { Position = new CPos(-6096, -3000, 0) };
			fullscreen.SetText("Fullscreen:");
			Add(fullscreen);

			var width = new UITextLine(FontManager.Pixel16) { Position = new CPos(-6096, -2300, 0) };
			if (Settings.Fullscreen)
				width.SetColor(new Color(128, 128, 128));
			width.SetText("Width:");
			Add(width);

			var height = new UITextLine(FontManager.Pixel16) { Position = new CPos(-6096, -1600, 0) };
			if (Settings.Fullscreen)
				height.SetColor(new Color(128, 128, 128));
			height.SetText("Height:");
			Add(height);

			fullscreenCheck = new CheckBox("wooden", Settings.Fullscreen, (ticked) =>
			{
				width.SetColor(ticked ? new Color(128, 128, 128) : Color.White);
				height.SetColor(ticked ? new Color(128, 128, 128) : Color.White);
			}) { Position = new CPos(-1536, -3000, 0) };
			Add(fullscreenCheck);

			widthWrite = new TextBox("wooden", 5, InputType.NUMBERS)
			{
				Position = new CPos(-2048, -2300, 0),
				Text = Settings.Width.ToString(),
				OnEnter = () =>
				{
					var parse = int.Parse(widthWrite.Text);
					if (parse < 640)
						widthWrite.Text = 640 + "";
					else if (parse > WindowInfo.ScreenWidth)
						widthWrite.Text = WindowInfo.ScreenWidth + "";
				}
			};
			Add(widthWrite);

			heightWrite = new TextBox("wooden", 5, InputType.NUMBERS)
			{
				Position = new CPos(-2048, -1600, 0),
				Text = Settings.Height.ToString(),
				OnEnter = () =>
				{
					var parse = int.Parse(heightWrite.Text);
					if (parse < 480)
						heightWrite.Text = 480 + "";
					else if (parse > WindowInfo.ScreenHeight)
						heightWrite.Text = WindowInfo.ScreenHeight + "";
				}
			};
			Add(heightWrite);

			// Graphics
			var vSync = new UITextLine(FontManager.Pixel16) { Position = new CPos(-512, -3000, 0) };
			vSync.SetText("Enable V-Sync:");
			Add(vSync);

			var pixeling = new UITextLine(FontManager.Pixel16) { Position = new CPos(-512, -2300, 0) };
			pixeling.SetText("Enable Pixeling:");
			Add(pixeling);

			var textshadow = new UITextLine(FontManager.Pixel16) { Position = new CPos(-512, -1600, 0) };
			textshadow.SetText("Enable text shadows:");
			Add(textshadow);

			vSyncCheck = new CheckBox("wooden", Settings.VSync, (b) =>
			{
				Settings.VSync = b;
				Window.SetVSync();
			}) { Position = new CPos(6656, -3000, 0) };
			Add(vSyncCheck);
			pixelingCheck = new CheckBox("wooden", Settings.EnablePixeling, (b) => Settings.EnablePixeling = b) { Position = new CPos(6656, -2300, 0) };
			Add(pixelingCheck);
			textshadowCheck = new CheckBox("wooden", Settings.EnableTextShadowing, (b) => Settings.EnableTextShadowing = b) { Position = new CPos(6656, -1600, 0) };
			Add(textshadowCheck);

			// Scrolling
			var scrollSpeed = new UITextLine(FontManager.Pixel16) { Position = new CPos(-6144, -600, 0) };
			scrollSpeed.SetText("Camera panning speed:");
			Add(scrollSpeed);
			var edgeScrolling = new UITextLine(FontManager.Pixel16) { Position = new CPos(-6144, 100, 0) };
			edgeScrolling.SetText("Edge Panning (0 = disabled):");
			Add(edgeScrolling);

			panningSlider = new SliderBar(4096, "wooden", () => Settings.ScrollSpeed = (int)(panningSlider.Value * 10))
			{
				Position = new CPos(5120, -600, 0),
				Value = Settings.ScrollSpeed / 10f
			};
			Add(panningSlider);
			edgePanningSlider = new SliderBar(4096, "wooden", () => Settings.EdgeScrolling = (int)(edgePanningSlider.Value * 10))
			{
				Position = new CPos(5120, 100, 0),
				Value = Settings.EdgeScrolling / 10f
			};
			Add(edgePanningSlider);

			// Additional features
			var frameLimiter = new UITextLine(FontManager.Pixel16) { Position = new CPos(-6144, 1000, 0) };
			frameLimiter.SetText("Framelimiter (0 = disabled):");
			Add(frameLimiter);

			frameLimiterWrite = new TextBox("wooden", 2, InputType.NUMBERS)
			{
				Position = new CPos(5120, 1000, 0),
				Text = Settings.FrameLimiter.ToString(),
				OnEnter = () =>
				{
					var number = int.Parse(frameLimiterWrite.Text);
					if (number > WindowInfo.ScreenRefreshRate)
						frameLimiterWrite.Text = WindowInfo.ScreenRefreshRate.ToString();
				}
			};
			Add(frameLimiterWrite);

			var developerMode = new UITextLine(FontManager.Pixel16) { Position = new CPos(-6144, 1900, 0) };
			developerMode.SetText("Enable Developermode:");
			Add(developerMode);
			developerModeCheck = new CheckBox("wooden", Settings.DeveloperMode, (b) =>
			{
				Settings.DeveloperMode = b;
			}) { Position = new CPos(5120, 1900, 0) };
			Add(developerModeCheck);

			// Volume
			var masterVol = new UITextLine(FontManager.Pixel16) { Position = new CPos(-6144, 2800, 0) };
			masterVol.SetText("Master Volume:");
			Add(masterVol);
			var effectVol = new UITextLine(FontManager.Pixel16) { Position = new CPos(-6144, 3700, 0) };
			effectVol.SetText("Effects Volume:");
			Add(effectVol);
			var musicVol = new UITextLine(FontManager.Pixel16) { Position = new CPos(-6144, 4600, 0) };
			musicVol.SetText("Music Volume:");
			Add(musicVol);

			masterVolumeSlider = new SliderBar(4096, "wooden", () =>
			{
				Settings.MasterVolume = (float)Math.Round(masterVolumeSlider.Value, 2);
				AudioController.Music.SetVolume();
			})
			{
				Position = new CPos(5120, 2800, 0),
				Value = Settings.MasterVolume
			};
			Add(masterVolumeSlider);
			effectVolumeSlider = new SliderBar(4096, "wooden", () =>
			{
				Settings.EffectsVolume = (float)Math.Round(effectVolumeSlider.Value, 2);
			})
			{
				Position = new CPos(5120, 3700, 0),
				Value = Settings.EffectsVolume
			};
			Add(effectVolumeSlider);
			musicVolumeSlider = new SliderBar(4096, "wooden", () =>
			{
				Settings.MusicVolume = (float)Math.Round(musicVolumeSlider.Value, 2);
				AudioController.Music.SetVolume();
			})
			{
				Position = new CPos(5120, 4600, 0),
				Value = Settings.MusicVolume
			};
			Add(musicVolumeSlider);

			var warning = new UITextLine(FontManager.Pixel16, TextOffset.MIDDLE)
			{
				Position = new CPos(0, 5450, 0),
				Color = Color.Red
			};
			warning.SetText("Some changes only take effect after restarting and can cause visual bugs.");
			Add(warning);

			Add(new Button("Apply", "wooden", Save) { Position = new CPos(-5120, 6144, 0) });
			Add(new Button("Save & Back", "wooden", () => game.ShowScreen(ScreenType.MENU)) { Position = new CPos(5120, 6144, 0) });
			Add(new Button("Key Bindings", "wooden", () => game.ShowScreen(ScreenType.KEYSETTINGS)) { Position = new CPos(0, 6144, 0) });
		}

		public override void Hide()
		{
			base.Hide();
			Save();
		}

		public void Save()
		{
			Settings.FrameLimiter = int.Parse(frameLimiterWrite.Text);
			Settings.ScrollSpeed = (int)(panningSlider.Value * 10);
			Settings.EdgeScrolling = (int)(edgePanningSlider.Value * 10);
			Settings.DeveloperMode = developerModeCheck.Checked;
			Settings.Fullscreen = fullscreenCheck.Checked;
			Settings.Width = int.Parse(widthWrite.Text);
			Settings.Height = int.Parse(heightWrite.Text);
			Settings.VSync = vSyncCheck.Checked;
			Settings.EnablePixeling = pixelingCheck.Checked;
			Settings.EnableTextShadowing = textshadowCheck.Checked;

			Settings.MasterVolume = (float)Math.Round(masterVolumeSlider.Value, 2);
			Settings.EffectsVolume = (float)Math.Round(effectVolumeSlider.Value, 2);
			Settings.MusicVolume = (float)Math.Round(musicVolumeSlider.Value, 2);
			AudioController.Music.SetVolume();

			Settings.Save();

			Window.UpdateScreen();

			game.AddInfoMessage(150, "Settings saved!");
			Log.WriteDebug("Saved settings.");
		}

		public override void KeyDown(Keys key, bool isControl, bool isShift, bool isAlt)
		{
			base.KeyDown(key, isControl, isShift, isAlt);

			if (key == Keys.Escape)
				game.ShowScreen(ScreenType.MENU);
		}
	}
}
