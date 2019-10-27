/*
 * User: Andreas
 * Date: 21.07.2018
 * Time: 16:13
 */
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
		readonly TextLine frameLimiter, scrollSpeed, edgeScrolling, developerMode, fullscreen, width, height, antiAliasing, pixeling, textshadow, warning;

		public bool Visible { get; private set; }

		public SettingsScreen(Game game) : base("Settings")
		{
			this.game = game;
			Title.Position = new CPos(0, -4096, 0);
			saved = new TextLine(new CPos(0, 6210, 0), IFont.Pixel16, TextLine.OffsetType.MIDDLE);
			saved.SetText("Save");

			widthWrite = TextBoxCreator.Create("wooden", new CPos(4000, -2300, 0), Settings.Width + "", 5, true);
			heightWrite = TextBoxCreator.Create("wooden", new CPos(4000, -1600, 0), Settings.Height + "", 5, true);

			fullscreen = new TextLine(new CPos(-5096, -3000, 0), IFont.Pixel16);
			fullscreen.SetText("Fullscreen:");
			fullscreenCheck = CheckBoxCreator.Create("wooden", new CPos(4000, -3000, 0), Settings.Fullscreen, (ticked) => { width.SetColor(ticked ? new Color(128, 128, 128) : Color.White); height.SetColor(ticked ? new Color(128, 128, 128) : Color.White); });

			width = new TextLine(new CPos(-5096, -2300, 0), IFont.Pixel16);
			if (Settings.Fullscreen)
				width.SetColor(new Color(128, 128, 128));
			width.SetText("Width:");
			height = new TextLine(new CPos(-5096, -1600, 0), IFont.Pixel16);
			if (Settings.Fullscreen)
				height.SetColor(new Color(128, 128, 128));
			height.SetText("Height:");

			scrollSpeed = new TextLine(new CPos(-5096, -600, 0), IFont.Pixel16);
			scrollSpeed.SetText("Scroll Speed:");
			scrollWrite = TextBoxCreator.Create("wooden", new CPos(4000, -600, 0), Settings.ScrollSpeed + "", 1, true);

			edgeScrolling = new TextLine(new CPos(-5096, 100, 0), IFont.Pixel16);
			edgeScrolling.SetText("Edge Scrolling (0 when disabled):");
			edgeScrollWrite = TextBoxCreator.Create("wooden", new CPos(4000, 100, 0), Settings.EdgeScrolling + "", 1, true);

			frameLimiter = new TextLine(new CPos(-5096, 1000, 0), IFont.Pixel16);
			frameLimiter.SetText("Framelimiter (0 when disabled):");
			frameLimiterWrite = TextBoxCreator.Create("wooden", new CPos(4000, 1000, 0), Settings.FrameLimiter + "", 2, true);

			developerMode = new TextLine(new CPos(-5096, 1900, 0), IFont.Pixel16);
			developerMode.SetText("Enable Developermode:");
			developerModeCheck = CheckBoxCreator.Create("wooden", new CPos(4000, 1900, 0), Settings.DeveloperMode);

			antiAliasing = new TextLine(new CPos(-5096, 2800, 0), IFont.Pixel16);
			antiAliasing.SetText("Enable Antialising:");
			antiAliasingCheck = CheckBoxCreator.Create("wooden", new CPos(4000, 2800, 0), Settings.AntiAliasing);

			pixeling = new TextLine(new CPos(-5096, 3700, 0), IFont.Pixel16);
			pixeling.SetText("Enable Pixeling:");
			pixelingCheck = CheckBoxCreator.Create("wooden", new CPos(4000, 3700, 0), Settings.EnablePixeling);

			textshadow = new TextLine(new CPos(-5096, 4400, 0), IFont.Pixel16);
			textshadow.SetText("Enable text shadows:");
			textshadowCheck = CheckBoxCreator.Create("wooden", new CPos(4000, 4400, 0), Settings.EnableTextShadowing);

			warning = new TextLine(new CPos(0, 5450, 0), IFont.Pixel16, TextLine.OffsetType.MIDDLE);
			warning.SetColor(Color.Red);
			warning.SetText("Some changes may only take effect after restarting and can cause visual bugs.");

			back = ButtonCreator.Create("wooden", new CPos(4096, 6144, 0), "Back", () => game.ChangeScreen(ScreenType.MENU));
			save = ButtonCreator.Create("wooden", new CPos(0, 6144, 0), "Save", Save);
			keys = ButtonCreator.Create("wooden", new CPos(-4096, 6144, 0), "Key Bindings", () => game.ChangeScreen(ScreenType.KEYSETTINGS));
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

			Settings.Save();

			Window.Current.SetScreen();
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
		}
	}
}
