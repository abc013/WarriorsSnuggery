using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using WarriorsSnuggery.Audio.Music;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.UI.Objects;

namespace WarriorsSnuggery.UI.Screens
{
	public class BaseSettingsScreen : Screen
	{
		readonly Game game;

		readonly CheckBox fullscreenCheck, vSyncCheck, developerModeCheck, pixelingCheck, textshadowCheck;
		readonly TextBox widthWrite, heightWrite, frameLimiterWrite;
		readonly SliderBar panningSlider, edgePanningSlider, masterVolumeSlider, effectVolumeSlider, musicVolumeSlider;

		public bool Visible { get; private set; }

		public BaseSettingsScreen(Game game) : base("")
		{
			this.game = game;
			Title.Position = new UIPos(0, -4096);
			Add(new SettingsChooser(game, new UIPos(0, -5120), ScreenType.BASESETTINGS, save));

			// Window
			var fullscreen = new UIText(FontManager.Default) { Position = new UIPos(-6096, -3000) };
			fullscreen.SetText("Fullscreen:");
			Add(fullscreen);

			var width = new UIText(FontManager.Default) { Position = new UIPos(-6096, -2300) };
			if (Settings.Fullscreen)
				width.Color = new Color(128, 128, 128);
			width.SetText("Width:");
			Add(width);

			var height = new UIText(FontManager.Default) { Position = new UIPos(-6096, -1600) };
			if (Settings.Fullscreen)
				height.Color = new Color(128, 128, 128);
			height.SetText("Height:");
			Add(height);

			fullscreenCheck = new CheckBox("wooden", Settings.Fullscreen, (ticked) =>
			{
				width.Color = ticked ? new Color(128, 128, 128) : Color.White;
				height.Color = ticked ? new Color(128, 128, 128) : Color.White;
			}) { Position = new UIPos(-1536, -3000) };
			Add(fullscreenCheck);

			widthWrite = new TextBox("wooden", 5, InputType.NUMBERS)
			{
				Position = new UIPos(-2048, -2300),
				Text = Settings.Width.ToString(),
				OnEnter = () =>
				{
					var parse = int.Parse(widthWrite.Text);
					if (parse < 640)
						widthWrite.Text = 640 + "";
					else if (parse > ScreenInfo.ScreenWidth)
						widthWrite.Text = ScreenInfo.ScreenWidth + "";
				}
			};
			Add(widthWrite);

			heightWrite = new TextBox("wooden", 5, InputType.NUMBERS)
			{
				Position = new UIPos(-2048, -1600),
				Text = Settings.Height.ToString(),
				OnEnter = () =>
				{
					var parse = int.Parse(heightWrite.Text);
					if (parse < 480)
						heightWrite.Text = 480 + "";
					else if (parse > ScreenInfo.ScreenHeight)
						heightWrite.Text = ScreenInfo.ScreenHeight + "";
				}
			};
			Add(heightWrite);

			// Graphics
			var vSync = new UIText(FontManager.Default) { Position = new UIPos(-512, -3000) };
			vSync.SetText("Enable V-Sync:");
			Add(vSync);

			var pixeling = new UIText(FontManager.Default) { Position = new UIPos(-512, -2300) };
			pixeling.SetText("Enable Pixeling:");
			Add(pixeling);

			var textshadow = new UIText(FontManager.Default) { Position = new UIPos(-512, -1600) };
			textshadow.SetText("Enable text shadows:");
			Add(textshadow);

			vSyncCheck = new CheckBox("wooden", Settings.VSync, (b) =>
			{
				Settings.VSync = b;
				Window.SetVSync();
			}) { Position = new UIPos(6656, -3000) };
			Add(vSyncCheck);
			pixelingCheck = new CheckBox("wooden", Settings.EnablePixeling, (b) => Settings.EnablePixeling = b) { Position = new UIPos(6656, -2300) };
			Add(pixelingCheck);
			textshadowCheck = new CheckBox("wooden", Settings.EnableTextShadowing, (b) => Settings.EnableTextShadowing = b) { Position = new UIPos(6656, -1600) };
			Add(textshadowCheck);

			// Scrolling
			var scrollSpeed = new UIText(FontManager.Default) { Position = new UIPos(-6144, -600) };
			scrollSpeed.SetText("Camera panning speed:");
			Add(scrollSpeed);
			var edgeScrolling = new UIText(FontManager.Default) { Position = new UIPos(-6144, 100) };
			edgeScrolling.SetText("Edge Panning (0 = disabled):");
			Add(edgeScrolling);

			panningSlider = new SliderBar(4096, "wooden", () => Settings.ScrollSpeed = (int)Math.Round(panningSlider.Value), 0, 10)
			{
				Position = new UIPos(5120, -600),
				Value = Settings.ScrollSpeed
			};
			Add(panningSlider);
			edgePanningSlider = new SliderBar(4096, "wooden", () => Settings.EdgeScrolling = (int)Math.Round(edgePanningSlider.Value), 0, 10)
			{
				Position = new UIPos(5120, 100),
				Value = Settings.EdgeScrolling
			};
			Add(edgePanningSlider);

			// Additional features
			var frameLimiter = new UIText(FontManager.Default) { Position = new UIPos(-6144, 1000) };
			frameLimiter.SetText("Framelimiter (0 = disabled):");
			Add(frameLimiter);

			frameLimiterWrite = new TextBox("wooden", 2, InputType.NUMBERS)
			{
				Position = new UIPos(5120, 1000),
				Text = Settings.FrameLimiter.ToString(),
				OnEnter = () =>
				{
					var number = int.Parse(frameLimiterWrite.Text);
					if (number > ScreenInfo.ScreenRefreshRate)
						frameLimiterWrite.Text = ScreenInfo.ScreenRefreshRate.ToString();
				}
			};
			Add(frameLimiterWrite);

			var developerMode = new UIText(FontManager.Default) { Position = new UIPos(-6144, 1900) };
			developerMode.SetText("Enable Developermode:");
			Add(developerMode);
			developerModeCheck = new CheckBox("wooden", Settings.DeveloperMode, (b) =>
			{
				Settings.DeveloperMode = b;
			}) { Position = new UIPos(5120, 1900) };
			Add(developerModeCheck);

			// Volume
			var masterVol = new UIText(FontManager.Default) { Position = new UIPos(-6144, 2800) };
			masterVol.SetText("Master Volume:");
			Add(masterVol);
			var effectVol = new UIText(FontManager.Default) { Position = new UIPos(-6144, 3700) };
			effectVol.SetText("Effects Volume:");
			Add(effectVol);
			var musicVol = new UIText(FontManager.Default) { Position = new UIPos(-6144, 4600) };
			musicVol.SetText("Music Volume:");
			Add(musicVol);

			masterVolumeSlider = new SliderBar(4096, "wooden", () =>
			{
				Settings.MasterVolume = (float)Math.Round(masterVolumeSlider.Value, 2);
				MusicController.UpdateVolume();
			}, tooltipDigits: 0, displayAsPercent: true)
			{
				Position = new UIPos(5120, 2800),
				Value = Settings.MasterVolume
			};
			Add(masterVolumeSlider);
			effectVolumeSlider = new SliderBar(4096, "wooden", () =>
			{
				Settings.EffectsVolume = (float)Math.Round(effectVolumeSlider.Value, 2);
			}, tooltipDigits: 0, displayAsPercent: true)
			{
				Position = new UIPos(5120, 3700),
				Value = Settings.EffectsVolume
			};
			Add(effectVolumeSlider);
			musicVolumeSlider = new SliderBar(4096, "wooden", () =>
			{
				Settings.MusicVolume = (float)Math.Round(musicVolumeSlider.Value, 2);
				MusicController.UpdateVolume();
			}, tooltipDigits: 0, displayAsPercent: true)
			{
				Position = new UIPos(5120, 4600),
				Value = Settings.MusicVolume
			};
			Add(musicVolumeSlider);

			var warning = new UIText(FontManager.Default, TextOffset.MIDDLE)
			{
				Position = new UIPos(0, 5450),
				Color = Color.Red
			};
			warning.SetText("Some changes only take effect after restarting and can cause visual bugs.");
			Add(warning);
		}

		public override void Hide()
		{
			base.Hide();
			save();
		}

		void save()
		{
			var height = Settings.Height;
			var width = Settings.Width;
			var fullscreen = Settings.Fullscreen;

			Settings.FrameLimiter = int.Parse(frameLimiterWrite.Text);
			Settings.ScrollSpeed = (int)Math.Round(panningSlider.Value);
			Settings.EdgeScrolling = (int)Math.Round(edgePanningSlider.Value);
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
			MusicController.UpdateVolume();

			Settings.Save();

			if ((Settings.Fullscreen != fullscreen) || (height != Settings.Height) || (width != Settings.Width))
			{
				Window.UpdateScreen();
				game.ScreenControl.ReloadScreenCache();
			}

			game.AddInfoMessage(150, "Settings saved!");
			Log.Debug("Saved settings.");
		}

		public override void KeyDown(Keys key, bool isControl, bool isShift, bool isAlt)
		{
			base.KeyDown(key, isControl, isShift, isAlt);

			if (key == Keys.Escape)
				game.ShowScreen(ScreenType.MENU);
		}
	}
}
