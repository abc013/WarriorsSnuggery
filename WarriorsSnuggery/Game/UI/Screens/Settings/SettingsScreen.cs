/*
 * User: Andreas
 * Date: 21.07.2018
 * Time: 16:13
 */
using System;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI
{
	public class SettingsScreen : Screen
	{
		readonly Game game;

		readonly Button back;
		readonly Button save;
		readonly Text saved;
		int savedTick;
		readonly Button keys;

		readonly CheckBox fullscreenCheck, antiAliasingCheck, developerModeCheck, pixelingCheck;
		readonly TextBox widthWrite, heightWrite, frameLimiterWrite, scrollWrite, edgeScrollWrite;
		readonly Text frameLimiter, scrollSpeed, edgeScrolling, developerMode, fullscreen, width, height, antiAliasing, pixeling, warning;

		public bool Visible { get; private set;}

		public SettingsScreen(Game game) : base("Settings")
		{
			this.game = game;
			Title.Position = new CPos(0,-4096, 0);
			saved = new Text(new CPos(0,6210, 0), IFont.Pixel16, Text.OffsetType.MIDDLE);
			saved.SetText("Save");

			back = ButtonCreator.Create("wooden", new CPos(4096, 6144, 0), "Back", () => game.ChangeScreen(ScreenType.MENU));
			save = ButtonCreator.Create("wooden", new CPos(0, 6144, 0), "Save", Save);
			keys = ButtonCreator.Create("wooden", new CPos(-4096, 6144, 0), "Key Bindings", () => game.ChangeScreen(ScreenType.KEYSETTINGS));

			fullscreenCheck = CheckBoxCreator.Create("wooden", new CPos(4000, -3000, 0),  Settings.Fullscreen, (ticked) => { width.SetColor(ticked ? new Color(128,128,128) : Color.White); height.SetColor(ticked ? new Color(128,128,128) : Color.White); });
			pixelingCheck = CheckBoxCreator.Create("wooden", new CPos(4000, 4900, 0), Settings.EnablePixeling);
			antiAliasingCheck = CheckBoxCreator.Create("wooden", new CPos(4000, 4200, 0), Settings.AntiAliasing);
			developerModeCheck = CheckBoxCreator.Create("wooden", new CPos(4000, 3500, 0), Settings.DeveloperMode);

			widthWrite = TextBoxCreator.Create("wooden", new CPos(4000, -2300, 0), Settings.Width + "", 5, true);
			heightWrite = TextBoxCreator.Create("wooden", new CPos(4000, -1600, 0), Settings.Height + "", 5, true);
			frameLimiterWrite = TextBoxCreator.Create("wooden", new CPos(4000, 2700, 0), Settings.FrameLimiter + "", 2, true);
			scrollWrite = TextBoxCreator.Create("wooden", new CPos(4000, 1000, 0), Settings.ScrollSpeed + "", 1, true);
			edgeScrollWrite = TextBoxCreator.Create("wooden", new CPos(4000, 1800, 0), Settings.EdgeScrolling + "", 1, true);

			frameLimiter = new Text(new CPos(-5096, 2700, 0), IFont.Pixel16);
			frameLimiter.SetText("Framelimiter (0 when disabled):");
			scrollSpeed = new Text(new CPos(-5096, 1000, 0), IFont.Pixel16);
			scrollSpeed.SetText("Scroll Speed:");
			edgeScrolling = new Text(new CPos(-5096, 1800, 0), IFont.Pixel16);
			edgeScrolling.SetText("Edge Scrolling (0 when disabled):");
			developerMode = new Text(new CPos(-5096, 3500, 0), IFont.Pixel16);
			developerMode.SetText("Enable Developermode:");

			fullscreen = new Text(new CPos(-5096, -3000, 0), IFont.Pixel16);
			fullscreen.SetText("Fullscreen:");
			width = new Text(new CPos(-5096, -2300, 0), IFont.Pixel16);
			if (Settings.Fullscreen)
				width.SetColor(new Color(128,128,128));
			width.SetText("Width:");
			height = new Text(new CPos(-5096, -1600, 0), IFont.Pixel16);
			if (Settings.Fullscreen)
				height.SetColor(new Color(128,128,128));
			height.SetText("Height:");
			antiAliasing = new Text(new CPos(-5096, 4200, 0), IFont.Pixel16);
			antiAliasing.SetText("Enable Antialising:");
			pixeling = new Text(new CPos(-5096, 4900, 0), IFont.Pixel16);
			pixeling.SetText("Enable Pixeling:");
			warning = new Text(new CPos(0, 5550, 0), IFont.Pixel16, Text.OffsetType.MIDDLE);
			warning.SetColor(Color.Red);
			warning.SetText("Some changes may only take effect after restarting and can cause visual bugs.");
		}

		public void Save()
		{
			savedTick = 15;
			using (var writer = new System.IO.StreamWriter(FileExplorer.MainDirectory + "WS.yaml"))
			{
				var limiter = int.Parse(frameLimiterWrite.Text);
				writer.WriteLine("FrameLimiter=" + limiter);
				Settings.FrameLimiter = limiter;

				var scroll = int.Parse(scrollWrite.Text);
				writer.WriteLine("ScrollSpeed=" + scroll);
				Settings.ScrollSpeed = scroll;
				var edgeScroll = int.Parse(edgeScrollWrite.Text);
				writer.WriteLine("EdgeScrolling=" + edgeScroll);
				Settings.EdgeScrolling = edgeScroll;

				var developer = developerModeCheck.Checked;
				writer.WriteLine("DeveloperMode=" + developer.GetHashCode());
				//Settings.DeveloperMode = developer; Crashes

				var isFullscreen = fullscreenCheck.Checked;
				writer.WriteLine("Fullscreen=" + isFullscreen.GetHashCode());
				Settings.Fullscreen = isFullscreen;

				var width = int.Parse(widthWrite.Text);
				writer.WriteLine("Width=" + width);
				Settings.Width = width;

				var height = int.Parse(heightWrite.Text);
				writer.WriteLine("Height=" + height);
				Settings.Height = height;

				Window.Current.SetScreen();

				var aliasing = antiAliasingCheck.Checked;
				writer.WriteLine("AntiAliasing=" + aliasing.GetHashCode());
				if (aliasing)
					MasterRenderer.EnableAliasing();
				else
					MasterRenderer.DisableAliasing();

				var pixeling = pixelingCheck.Checked;
				writer.WriteLine("EnablePixeling=" + pixeling.GetHashCode());
				Settings.EnablePixeling = pixeling;

				writer.WriteLine("FirstStarted=0");

				writer.WriteLine("Keys=");
				foreach(var key in Settings.KeyDictionary)
				{
					writer.WriteLine("\t" + key.Key + "=" + key.Value);
				}

				writer.Flush();
				writer.Close();
			}
			game.AddInfoMessage(150, "Settings saved!");
		}

		public override void Render()
		{
			base.Render();

			fullscreenCheck.Render();
			pixelingCheck.Render();
			antiAliasingCheck.Render();
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
			developerModeCheck.Tick();

			widthWrite.Tick();
			heightWrite.Tick();
			frameLimiterWrite.Tick();
			scrollWrite.Tick();
			edgeScrollWrite.Tick();

			frameLimiter.Tick();
			scrollSpeed.Tick();
			edgeScrolling.Tick();
			developerMode.Tick();
			fullscreen.Tick();
			width.Tick();
			height.Tick();
			antiAliasing.Tick();
			pixeling.Tick();
			warning.Tick();

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
			warning.Dispose();
		}
	}
}
