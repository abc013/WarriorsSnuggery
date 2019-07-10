using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI
{
	class KeyboardScreen : Screen
	{
		readonly Game game;

		readonly Button save;
		readonly Button back;

		readonly KeyboardButton up, down, right, left, pause, camUp, camDown, camRight, camLeft, @lock;
		readonly TextLine tMove, tPause, tCam, tLock;
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

			tPause = new TextLine(new CPos(-2048, -3072, 0), IFont.Pixel16);
			tPause.SetText("Pause/unpause");
			pause = new KeyboardButton(new CPos(2048, -3072, 0), Settings.KeyDictionary["Pause"][0], texture, IFont.Pixel16, Color.Black);
			tLock = new TextLine(new CPos(-2048, -2048, 0), IFont.Pixel16);
			tLock.SetText("Toggle cam lock");
			@lock = new KeyboardButton(new CPos(2048, -2048, 0), Settings.KeyDictionary["CameraLock"][0], texture, IFont.Pixel16, Color.Black);

			tMove = new TextLine(new CPos(-2048, -256, 0), IFont.Pixel16);
			tMove.SetText("Movement");
			up = new KeyboardButton(new CPos(2048, -576, 0), Settings.KeyDictionary["MoveUp"][0], texture, IFont.Pixel16, Color.Black);
			down = new KeyboardButton(new CPos(2048, 0, 1), Settings.KeyDictionary["MoveDown"][0], texture, IFont.Pixel16, Color.Black);
			left = new KeyboardButton(new CPos(1276, 0, 1), Settings.KeyDictionary["MoveLeft"][0], texture, IFont.Pixel16, Color.Black);
			right = new KeyboardButton(new CPos(2806, 0, 1), Settings.KeyDictionary["MoveRight"][0], texture, IFont.Pixel16, Color.Black);

			tCam = new TextLine(new CPos(-2048, 1748, 0), IFont.Pixel16);
			tCam.SetText("Camera");
			camUp = new KeyboardButton(new CPos(2048, 1448, 0), Settings.KeyDictionary["CameraUp"][0], texture, IFont.Pixel16, Color.Black);
			camDown = new KeyboardButton(new CPos(2048, 2048, 1), Settings.KeyDictionary["CameraDown"][0], texture, IFont.Pixel16, Color.Black);
			camLeft = new KeyboardButton(new CPos(1276, 2048, 1), Settings.KeyDictionary["CameraLeft"][0], texture, IFont.Pixel16, Color.Black);
			camRight = new KeyboardButton(new CPos(2806, 2048, 1), Settings.KeyDictionary["CameraRight"][0], texture, IFont.Pixel16, Color.Black);
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
				Settings.KeyDictionary.Add("Pause", pause.Key + "");
				Settings.KeyDictionary.Add("CameraLock", @lock.Key + "");
				Settings.KeyDictionary.Add("MoveUp", up.Key + "");
				Settings.KeyDictionary.Add("MoveDown", down.Key + "");
				Settings.KeyDictionary.Add("MoveLeft", left.Key + "");
				Settings.KeyDictionary.Add("MoveRight", right.Key + "");
				Settings.KeyDictionary.Add("CameraUp", camUp.Key + "");
				Settings.KeyDictionary.Add("CameraDown", camDown.Key + "");
				Settings.KeyDictionary.Add("CameraLeft", camLeft.Key + "");
				Settings.KeyDictionary.Add("CameraRight", camRight.Key + "");

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
			tPause.Tick();
			pause.Tick();
			tLock.Tick();
			@lock.Tick();

			tMove.Tick();
			up.Tick();
			down.Tick();
			left.Tick();
			right.Tick();

			tCam.Tick();
			camUp.Tick();
			camDown.Tick();
			camLeft.Tick();
			camRight.Tick();

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
			tPause.Render();
			pause.Render();
			tLock.Render();
			@lock.Render();

			tMove.Render();
			up.Render();
			down.Render();
			left.Render();
			right.Render();

			tCam.Render();
			camUp.Render();
			camDown.Render();
			camLeft.Render();
			camRight.Render();

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
			tPause.Dispose();
			pause.Dispose();
			tLock.Dispose();
			@lock.Dispose();

			tMove.Dispose();
			up.Dispose();
			down.Dispose();
			left.Dispose();
			right.Dispose();

			tCam.Dispose();
			camUp.Dispose();
			camDown.Dispose();
			camLeft.Dispose();
			camRight.Dispose();

			save.Dispose();
			back.Dispose();
		}
	}
}
