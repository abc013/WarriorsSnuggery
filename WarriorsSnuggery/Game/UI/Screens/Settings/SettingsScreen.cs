using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI
{
	public class SettingsScreen : Screen
	{
		readonly Game game;

		readonly Button back;
		readonly Button save;
		readonly TextLine saved;
		int savedTick;
		readonly Button keys;

		readonly CheckBox fullscreenCheck, antiAliasingCheck, developerModeCheck, pixelingCheck, textshadowCheck;
		readonly TextBox widthWrite, heightWrite, frameLimiterWrite, scrollWrite, edgeScrollWrite;
		readonly TextLine frameLimiter, scrollSpeed, edgeScrolling, developerMode, fullscreen, width, height, masterVol, effectVol, musicVol, antiAliasing, pixeling, textshadow, warning;
		readonly SliderBar masterVolumeSlider, effectVolumeSlider, musicVolumeSlider;

		public bool Visible { get; private set; }

		public SettingsScreen(Game game) : base("Settings")
		{
			this.game = game;
			Title.Position = new CPos(0, -4096, 0);
			saved = new TextLine(new CPos(0, 6210, 0), IFont.Pixel16, TextLine.OffsetType.MIDDLE);
			saved.SetText("Save");

			// Window
			fullscreen = new TextLine(new CPos(-6096, -3000, 0), IFont.Pixel16);
			fullscreen.SetText("Fullscreen:");
			fullscreenCheck = CheckBoxCreator.Create("wooden", new CPos(-1024, -3000, 0), Settings.Fullscreen, (ticked) => { width.SetColor(ticked ? new Color(128, 128, 128) : Color.White); height.SetColor(ticked ? new Color(128, 128, 128) : Color.White); });

			width = new TextLine(new CPos(-6096, -2300, 0), IFont.Pixel16);
			if (Settings.Fullscreen)
				width.SetColor(new Color(128, 128, 128));
			width.SetText("Width:");
			widthWrite = TextBoxCreator.Create("wooden", new CPos(-1536, -2300, 0), Settings.Width + "", 5, true);

			height = new TextLine(new CPos(-6096, -1600, 0), IFont.Pixel16);
			if (Settings.Fullscreen)
				height.SetColor(new Color(128, 128, 128));
			height.SetText("Height:");
			heightWrite = TextBoxCreator.Create("wooden", new CPos(-1536, -1600, 0), Settings.Height + "", 5, true);

			// Graphics
			antiAliasing = new TextLine(new CPos(128, -3000, 0), IFont.Pixel16);
			antiAliasing.SetText("Enable Antialising:");
			antiAliasingCheck = CheckBoxCreator.Create("wooden", new CPos(5024, -3000, 0), Settings.AntiAliasing);

			pixeling = new TextLine(new CPos(128, -2300, 0), IFont.Pixel16);
			pixeling.SetText("Enable Pixeling:");
			pixelingCheck = CheckBoxCreator.Create("wooden", new CPos(5024, -2300, 0), Settings.EnablePixeling);

			textshadow = new TextLine(new CPos(128, -1600, 0), IFont.Pixel16);
			textshadow.SetText("Enable text shadows:");
			textshadowCheck = CheckBoxCreator.Create("wooden", new CPos(5024, -1600, 0), Settings.EnableTextShadowing);

			// Scrolling
			scrollSpeed = new TextLine(new CPos(-6096, -600, 0), IFont.Pixel16);
			scrollSpeed.SetText("Scroll Speed:");
			scrollWrite = TextBoxCreator.Create("wooden", new CPos(5024, -600, 0), Settings.ScrollSpeed + "", 1, true);

			edgeScrolling = new TextLine(new CPos(-6096, 100, 0), IFont.Pixel16);
			edgeScrolling.SetText("Edge Scrolling (0 when disabled):");
			edgeScrollWrite = TextBoxCreator.Create("wooden", new CPos(5024, 100, 0), Settings.EdgeScrolling + "", 1, true);
			
			// Additional features
			frameLimiter = new TextLine(new CPos(-6096, 1000, 0), IFont.Pixel16);
			frameLimiter.SetText("Framelimiter (0 when disabled):");
			frameLimiterWrite = TextBoxCreator.Create("wooden", new CPos(5024, 1000, 0), Settings.FrameLimiter + "", 2, true);

			developerMode = new TextLine(new CPos(-6096, 1900, 0), IFont.Pixel16);
			developerMode.SetText("Enable Developermode:");
			developerModeCheck = CheckBoxCreator.Create("wooden", new CPos(5024, 1900, 0), Settings.DeveloperMode);

			masterVol = new TextLine(new CPos(-6096, 2800, 0), IFont.Pixel16);
			masterVol.SetText("Master Volume:");
			effectVol = new TextLine(new CPos(-6096, 3700, 0), IFont.Pixel16);
			effectVol.SetText("Effects Volume:");
			musicVol = new TextLine(new CPos(-6096, 4600, 0), IFont.Pixel16);
			musicVol.SetText("Music Volume:");
			masterVolumeSlider = new SliderBar(new CPos(5120, 2800, 0), 100, PanelManager.Get("wooden"))
			{
				Value = Settings.MasterVolume
			};
			effectVolumeSlider = new SliderBar(new CPos(5120, 3700, 0), 100, PanelManager.Get("wooden"))
			{
				Value = Settings.EffectsVolume
			};
			musicVolumeSlider = new SliderBar(new CPos(5120, 4600, 0), 100, PanelManager.Get("wooden"))
			{
				Value = Settings.MusicVolume
			};

			warning = new TextLine(new CPos(0, 5450, 0), IFont.Pixel16, TextLine.OffsetType.MIDDLE);
			warning.SetColor(Color.Red);
			warning.SetText("Some changes may only take effect after restarting and can cause visual bugs.");

			back = ButtonCreator.Create("wooden", new CPos(5120, 6144, 0), "Back", () => game.ChangeScreen(ScreenType.MENU));
			keys = ButtonCreator.Create("wooden", new CPos(0, 6144, 0), "Key Bindings", () => game.ChangeScreen(ScreenType.KEYSETTINGS));
			save = ButtonCreator.Create("wooden", new CPos(-5120, 6144, 0), "Save", Save);
		}

		public void Save()
		{
			Settings.FrameLimiter = int.Parse(frameLimiterWrite.Text);
			if (Settings.FrameLimiter == 0 || Settings.FrameLimiter > OpenTK.DisplayDevice.Default.RefreshRate)
				Settings.FrameLimiter = (int)OpenTK.DisplayDevice.Default.RefreshRate;

			Settings.ScrollSpeed = int.Parse(scrollWrite.Text);
			Settings.EdgeScrolling = int.Parse(edgeScrollWrite.Text);
			Settings.DeveloperMode = developerModeCheck.Checked;
			Settings.Fullscreen = fullscreenCheck.Checked;
			Settings.Width = int.Parse(widthWrite.Text);
			Settings.Height = int.Parse(heightWrite.Text);
			Settings.AntiAliasing = antiAliasingCheck.Checked;
			Settings.EnablePixeling = pixelingCheck.Checked;
			Settings.EnableTextShadowing = textshadowCheck.Checked;

			Settings.MasterVolume = masterVolumeSlider.Value;
			Settings.EffectsVolume = effectVolumeSlider.Value;
			Settings.MusicVolume = musicVolumeSlider.Value;

			Settings.Save();

			Window.UpdateScreen();
			if (Settings.AntiAliasing)
				MasterRenderer.EnableAliasing();
			else
				MasterRenderer.DisableAliasing();

			savedTick = 15;
			game.AddInfoMessage(150, "Settings saved!");
		}

		public override void Render()
		{
			base.Render();

			fullscreenCheck.Render();
			pixelingCheck.Render();
			antiAliasingCheck.Render();
			textshadowCheck.Render();
			developerModeCheck.Render();

			widthWrite.Render();
			heightWrite.Render();
			frameLimiterWrite.Render();
			scrollWrite.Render();
			edgeScrollWrite.Render();

			masterVolumeSlider.Render();
			effectVolumeSlider.Render();
			musicVolumeSlider.Render();

			masterVol.Render();
			effectVol.Render();
			musicVol.Render();

			frameLimiter.Render();
			scrollSpeed.Render();
			edgeScrolling.Render();
			developerMode.Render();
			fullscreen.Render();
			width.Render();
			height.Render();
			antiAliasing.Render();
			pixeling.Render();
			textshadow.Render();
			warning.Render();

			back.Render();
			save.Render();
			if (savedTick > 0)
			{
				savedTick--;
				saved.Scale = 1.7f - savedTick / 15f;
				saved.SetColor(new Color(1f, 1f, 1f, savedTick / 15f));
				saved.Render();
			}
			keys.Render();
		}

		public override void Tick()
		{
			base.Tick();

			fullscreenCheck.Tick();
			pixelingCheck.Tick();
			antiAliasingCheck.Tick();
			textshadowCheck.Tick();
			developerModeCheck.Tick();

			widthWrite.Tick();
			heightWrite.Tick();
			frameLimiterWrite.Tick();
			scrollWrite.Tick();
			edgeScrollWrite.Tick();

			masterVolumeSlider.Tick();
			effectVolumeSlider.Tick();
			musicVolumeSlider.Tick();

			back.Tick();
			save.Tick();
			keys.Tick();

			if (KeyInput.IsKeyDown("escape", 10))
			{
				game.ChangeScreen(ScreenType.MENU);
			}
		}

		public override void Dispose()
		{
			base.Dispose();
			keys.Dispose();

			back.Dispose();
			save.Dispose();
			saved.Dispose();
			keys.Dispose();

			fullscreenCheck.Dispose();
			pixelingCheck.Dispose();
			antiAliasingCheck.Dispose();
			textshadowCheck.Dispose();
			developerModeCheck.Dispose();

			widthWrite.Dispose();
			heightWrite.Dispose();
			frameLimiterWrite.Dispose();
			scrollWrite.Dispose();
			edgeScrollWrite.Dispose();

			frameLimiter.Dispose();
			scrollSpeed.Dispose();
			edgeScrolling.Dispose();
			developerMode.Dispose();
			fullscreen.Dispose();
			width.Dispose();
			height.Dispose();
			antiAliasing.Dispose();
			pixeling.Dispose();
			textshadow.Dispose();
			warning.Dispose();

			masterVolumeSlider.Dispose();
			effectVolumeSlider.Dispose();
			musicVolumeSlider.Dispose();

			masterVol.Dispose();
			effectVol.Dispose();
			musicVol.Dispose();
		}
	}
}
