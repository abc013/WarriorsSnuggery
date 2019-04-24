using System;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI
{
	class KeyboardScreen : Screen
	{
		readonly Game game;

		readonly Button save;
		readonly Button back;

		public KeyboardButton Up, Down, Right, Left, Pause, CamUp, CamDown, CamRight, CamLeft, Lock;
		readonly TextLine saved;
		int savedTick;

		public KeyboardScreen(Game game) : base("Keyboard")
		{
			this.game = game;

			Title.Position = new CPos(0, -4096, 0);
			saved = new TextLine(new CPos(-2048, 6210, 0), IFont.Pixel16, TextLine.OffsetType.MIDDLE);
			saved.SetText("Save");

			back = ButtonCreator.Create("wooden", new CPos(2048, 6144, 0), "Back", () => game.ChangeScreen(ScreenType.SETTINGS));
			save = ButtonCreator.Create("wooden", new CPos(-2048, 6144, 0), "Save", Save);

			var texture = new TextureInfo("keyboard", TextureType.ANIMATION, 10, 24, 24);

			Pause = new KeyboardButton(new CPos(2048, -3072, 0), Settings.KeyDictionary["Pause"][0], texture, IFont.Pixel16, Color.Black);
			Lock = new KeyboardButton(new CPos(2048, -2048, 0), Settings.KeyDictionary["CameraLock"][0], texture, IFont.Pixel16, Color.Black);

			Up = new KeyboardButton(new CPos(2048, -576, 0), Settings.KeyDictionary["MoveUp"][0], texture, IFont.Pixel16, Color.Black);
			Down = new KeyboardButton(new CPos(2048, 0, 1), Settings.KeyDictionary["MoveDown"][0], texture, IFont.Pixel16, Color.Black);
			Left = new KeyboardButton(new CPos(1276, 0, 1), Settings.KeyDictionary["MoveLeft"][0], texture, IFont.Pixel16, Color.Black);
			Right = new KeyboardButton(new CPos(2806, 0, 1), Settings.KeyDictionary["MoveRight"][0], texture, IFont.Pixel16, Color.Black);

			CamUp = new KeyboardButton(new CPos(2048, 1448, 0), Settings.KeyDictionary["CameraUp"][0], texture, IFont.Pixel16, Color.Black);
			CamDown = new KeyboardButton(new CPos(2048, 2048, 1), Settings.KeyDictionary["CameraDown"][0], texture, IFont.Pixel16, Color.Black);
			CamLeft = new KeyboardButton(new CPos(1276, 2048, 1), Settings.KeyDictionary["CameraLeft"][0], texture, IFont.Pixel16, Color.Black);
			CamRight = new KeyboardButton(new CPos(2806, 2048, 1), Settings.KeyDictionary["CameraRight"][0], texture, IFont.Pixel16, Color.Black);
		}

		void Save()
		{
			savedTick = 15;

			using (var writer = new System.IO.StreamWriter(FileExplorer.MainDirectory + "WS.yaml"))
			{
				writer.WriteLine("FrameLimiter=" + Settings.FrameLimiter);
				writer.WriteLine("ScrollSpeed=" + Settings.ScrollSpeed);
				writer.WriteLine("EdgeScrolling=" + Settings.EdgeScrolling);
				writer.WriteLine("DeveloperMode=" + Settings.DeveloperMode.GetHashCode());
				writer.WriteLine("Fullscreen=" + Settings.Fullscreen.GetHashCode());
				writer.WriteLine("Width=" + Settings.Width);
				writer.WriteLine("Height=" + Settings.Height);
				writer.WriteLine("AntiAliasing=" + Settings.AntiAliasing.GetHashCode());
				writer.WriteLine("EnablePixeling=" + Settings.EnablePixeling.GetHashCode());
				writer.WriteLine("FirstStarted=" + 0);

				Settings.KeyDictionary.Clear();
				Settings.KeyDictionary.Add("Pause", Pause.Key + "");
				Settings.KeyDictionary.Add("CameraLock", Lock.Key + "");
				Settings.KeyDictionary.Add("MoveUp", Up.Key + "");
				Settings.KeyDictionary.Add("MoveDown", Down.Key + "");
				Settings.KeyDictionary.Add("MoveLeft", Left.Key + "");
				Settings.KeyDictionary.Add("MoveRight", Right.Key + "");
				Settings.KeyDictionary.Add("CameraUp", CamUp.Key + "");
				Settings.KeyDictionary.Add("CameraDown", CamDown.Key + "");
				Settings.KeyDictionary.Add("CameraLeft", CamLeft.Key + "");
				Settings.KeyDictionary.Add("CameraRight", CamRight.Key + "");

				writer.WriteLine("Keys=");
				foreach (var key in Settings.KeyDictionary)
				{
					writer.WriteLine("\t" + key.Key + "=" + key.Value);
				}

				writer.Flush();
				writer.Close();
			}

			game.AddInfoMessage(150, "Keys Saved!");
		}

		public override void Tick()
		{
			base.Tick();
			Pause.Tick();
			Lock.Tick();

			Up.Tick();
			Down.Tick();
			Left.Tick();
			Right.Tick();

			CamUp.Tick();
			CamDown.Tick();
			CamLeft.Tick();
			CamRight.Tick();

			save.Tick();
			back.Tick();

			if (KeyInput.IsKeyDown("escape", 10))
			{
				game.ChangeScreen(ScreenType.SETTINGS);
			}
		}

		public override void Render()
		{
			base.Render();
			Pause.Render();
			Lock.Render();

			Up.Render();
			Down.Render();
			Left.Render();
			Right.Render();

			CamUp.Render();
			CamDown.Render();
			CamLeft.Render();
			CamRight.Render();

			save.Render();
			if (savedTick > 0)
			{
				savedTick--;
				saved.Scale = 1.7f - savedTick / 15f;
				saved.SetColor(new Color(1f, 1f, 1f, savedTick / 15f));
				saved.Render();
			}
			back.Render();
		}

		public override void Dispose()
		{
			base.Dispose();
			Pause.Dispose();
			Lock.Dispose();

			Up.Dispose();
			Down.Dispose();
			Left.Dispose();
			Right.Dispose();

			CamUp.Dispose();
			CamDown.Dispose();
			CamLeft.Dispose();
			CamRight.Dispose();

			save.Dispose();
			back.Dispose();
		}
	}
}
