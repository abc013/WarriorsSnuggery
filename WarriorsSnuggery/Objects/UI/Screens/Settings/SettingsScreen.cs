using System;
using WarriorsSnuggery.Audio;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI
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
			var fullscreen = new UITextLine(new CPos(-6096, -3000, 0), FontManager.Pixel16);
			fullscreen.SetText("Fullscreen:");
			Content.Add(fullscreen);
			var width = new UITextLine(new CPos(-6096, -2300, 0), FontManager.Pixel16);
			if (Settings.Fullscreen)
				width.SetColor(new Color(128, 128, 128));
			width.SetText("Width:");
			Content.Add(width);
			var height = new UITextLine(new CPos(-6096, -1600, 0), FontManager.Pixel16);
			if (Settings.Fullscreen)
				height.SetColor(new Color(128, 128, 128));
			height.SetText("Height:");
			Content.Add(height);

			fullscreenCheck = CheckBoxCreator.Create("wooden", new CPos(-1536, -3000, 0), Settings.Fullscreen, (ticked) =>
			{
				width.SetColor(ticked ? new Color(128, 128, 128) : Color.White);
				height.SetColor(ticked ? new Color(128, 128, 128) : Color.White);
			});
			Content.Add(fullscreenCheck);
			widthWrite = new TextBox(new CPos(-2048, -2300, 0), Settings.Width + "", "wooden", 5, true);
			widthWrite.OnEnter = () =>
			{
				var parse = int.Parse(widthWrite.Text);
				if (parse < 640)
					widthWrite.Text = 640 + "";
				else if (parse > WindowInfo.ScreenWidth)
					widthWrite.Text = WindowInfo.ScreenWidth + "";
			};
			Content.Add(widthWrite);
			heightWrite = new TextBox(new CPos(-2048, -1600, 0), Settings.Height + "", "wooden", 5, true);
			heightWrite.OnEnter = () =>
			{
				var parse = int.Parse(heightWrite.Text);
				if (parse < 480)
					heightWrite.Text = 480 + "";
				else if (parse > WindowInfo.ScreenHeight)
					heightWrite.Text = WindowInfo.ScreenHeight + "";
			};
			Content.Add(heightWrite);

			// Graphics
			var vSync = new UITextLine(new CPos(-512, -3000, 0), FontManager.Pixel16);
			vSync.SetText("Enable V-Sync:");
			Content.Add(vSync);
			var pixeling = new UITextLine(new CPos(-512, -2300, 0), FontManager.Pixel16);
			pixeling.SetText("Enable Pixeling:");
			Content.Add(pixeling);
			var textshadow = new UITextLine(new CPos(-512, -1600, 0), FontManager.Pixel16);
			textshadow.SetText("Enable text shadows:");
			Content.Add(textshadow);

			vSyncCheck = CheckBoxCreator.Create("wooden", new CPos(6656, -3000, 0), Settings.VSync, (b) =>
			{
				Settings.VSync = b;
				Window.SetVSync();
			});
			Content.Add(vSyncCheck);
			pixelingCheck = CheckBoxCreator.Create("wooden", new CPos(6656, -2300, 0), Settings.EnablePixeling, (b) => Settings.EnablePixeling = b);
			Content.Add(pixelingCheck);
			textshadowCheck = CheckBoxCreator.Create("wooden", new CPos(6656, -1600, 0), Settings.EnableTextShadowing, (b) => Settings.EnableTextShadowing = b);
			Content.Add(textshadowCheck);

			// Scrolling
			var scrollSpeed = new UITextLine(new CPos(-6144, -600, 0), FontManager.Pixel16);
			scrollSpeed.SetText("Camera panning speed:");
			Content.Add(scrollSpeed);
			var edgeScrolling = new UITextLine(new CPos(-6144, 100, 0), FontManager.Pixel16);
			edgeScrolling.SetText("Edge Panning (0 = disabled):");
			Content.Add(edgeScrolling);

			panningSlider = new SliderBar(new CPos(5120, -600, 0), 4096, PanelManager.Get("wooden"), () => Settings.ScrollSpeed = (int)(panningSlider.Value * 10))
			{
				Value = Settings.ScrollSpeed / 10f
			};
			Content.Add(panningSlider);
			edgePanningSlider = new SliderBar(new CPos(5120, 100, 0), 4096, PanelManager.Get("wooden"), () => Settings.EdgeScrolling = (int)(edgePanningSlider.Value * 10))
			{
				Value = Settings.EdgeScrolling / 10f
			};
			Content.Add(edgePanningSlider);

			// Additional features
			var frameLimiter = new UITextLine(new CPos(-6144, 1000, 0), FontManager.Pixel16);
			frameLimiter.SetText("Framelimiter (0 = disabled):");
			Content.Add(frameLimiter);

			frameLimiterWrite = new TextBox(new CPos(5120, 1000, 0), Settings.FrameLimiter + "", "wooden", 2, true);
			frameLimiterWrite.OnEnter = () =>
			{
				var number = int.Parse(frameLimiterWrite.Text);
				if (number > WindowInfo.ScreenRefreshRate)
					frameLimiterWrite.Text = WindowInfo.ScreenRefreshRate.ToString();
			};
			Content.Add(frameLimiterWrite);

			var developerMode = new UITextLine(new CPos(-6144, 1900, 0), FontManager.Pixel16);
			developerMode.SetText("Enable Developermode:");
			Content.Add(developerMode);
			developerModeCheck = CheckBoxCreator.Create("wooden", new CPos(5120, 1900, 0), Settings.DeveloperMode, (b) =>
			{
				Settings.DeveloperMode = b;
			});
			Content.Add(developerModeCheck);

			// Volume
			var masterVol = new UITextLine(new CPos(-6144, 2800, 0), FontManager.Pixel16);
			masterVol.SetText("Master Volume:");
			Content.Add(masterVol);
			var effectVol = new UITextLine(new CPos(-6144, 3700, 0), FontManager.Pixel16);
			effectVol.SetText("Effects Volume:");
			Content.Add(effectVol);
			var musicVol = new UITextLine(new CPos(-6144, 4600, 0), FontManager.Pixel16);
			musicVol.SetText("Music Volume:");
			Content.Add(musicVol);

			masterVolumeSlider = new SliderBar(new CPos(5120, 2800, 0), 4096, PanelManager.Get("wooden"), () =>
			{
				Settings.MasterVolume = (float)Math.Round(masterVolumeSlider.Value, 2);
				AudioController.Music.SetVolume();
			})
			{
				Value = Settings.MasterVolume
			};
			Content.Add(masterVolumeSlider);
			effectVolumeSlider = new SliderBar(new CPos(5120, 3700, 0), 4096, PanelManager.Get("wooden"), () =>
			{
				Settings.EffectsVolume = (float)Math.Round(effectVolumeSlider.Value, 2);
			})
			{
				Value = Settings.EffectsVolume
			};
			Content.Add(effectVolumeSlider);
			musicVolumeSlider = new SliderBar(new CPos(5120, 4600, 0), 4096, PanelManager.Get("wooden"), () =>
			{
				Settings.MusicVolume = (float)Math.Round(musicVolumeSlider.Value, 2);
				AudioController.Music.SetVolume();
			})
			{
				Value = Settings.MusicVolume
			};
			Content.Add(musicVolumeSlider);

			var warning = new UITextLine(new CPos(0, 5450, 0), FontManager.Pixel16, TextOffset.MIDDLE);
			warning.SetColor(Color.Red);
			warning.SetText("Some changes only take effect after restarting and can cause visual bugs.");
			Content.Add(warning);

			Content.Add(new Button(new CPos(-5120, 6144, 0), "Apply", "wooden", Save));
			Content.Add(new Button(new CPos(5120, 6144, 0), "Save & Back", "wooden", () => game.ChangeScreen(ScreenType.MENU)));
			Content.Add(new Button(new CPos(0, 6144, 0), "Key Bindings", "wooden", () => game.ChangeScreen(ScreenType.KEYSETTINGS)));
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

		public override void Tick()
		{
			base.Tick();

			if (KeyInput.IsKeyDown("escape", 10))
				game.ChangeScreen(ScreenType.MENU);
		}
	}
}
