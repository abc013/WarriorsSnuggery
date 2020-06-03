using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI
{
	class KeyboardScreen : Screen
	{
		readonly Game game;

		readonly KeyboardButton up, down, right, left, above, below, pause, camUp, camDown, camRight, camLeft, @lock;

		public KeyboardScreen(Game game) : base("Key Bindings")
		{
			this.game = game;

			Title.Position = new CPos(0, -4096, 0);

			Content.Add(ButtonCreator.Create("wooden", new CPos(0, 6144, 0), "Save & Back", () => game.ChangeScreen(ScreenType.SETTINGS)));

			var type = PanelManager.Get("wooden");

			var tPause = new TextLine(new CPos(-1024, -3072, 0), FontManager.Pixel16, TextLine.OffsetType.RIGHT);
			tPause.SetText("Pause/unpause");
			Content.Add(tPause);
			pause = new KeyboardButton(new CPos(1536, -3072, 0), Settings.KeyDictionary["Pause"], Color.White, type);
			Content.Add(pause);
			var tLock = new TextLine(new CPos(-1024, -2048, 0), FontManager.Pixel16, TextLine.OffsetType.RIGHT);
			tLock.SetText("Toggle camera lock");
			Content.Add(tLock);
			@lock = new KeyboardButton(new CPos(1536, -2048, 0), Settings.KeyDictionary["CameraLock"], Color.White, type);
			Content.Add(@lock);

			var font = FontManager.Pixel16;

			var tMove = new TextLine(new CPos(-3072, -1024, 0), FontManager.Pixel16, TextLine.OffsetType.RIGHT);
			tMove.SetText("Movement");
			Content.Add(tMove);
			var line = 0;
			foreach (var dir in new[] { "Up", "Down", "Left", "Right", "Above", "Below" })
			{
				var text = new TextLine(new CPos(-1024, -1024 + line * 640, 0), font, TextLine.OffsetType.RIGHT);
				text.SetText(dir);
				line++;
				Content.Add(text);
			}
			up = new KeyboardButton(new CPos(1536, -1024, 0), Settings.KeyDictionary["MoveUp"], Color.White, type);
			Content.Add(up);
			down = new KeyboardButton(new CPos(1536, -1024 + 640, 1), Settings.KeyDictionary["MoveDown"], Color.White, type);
			Content.Add(down);
			left = new KeyboardButton(new CPos(1536, -1024 + 1280, 1), Settings.KeyDictionary["MoveLeft"], Color.White, type);
			Content.Add(left);
			right = new KeyboardButton(new CPos(1536, -1024 + 1920, 0), Settings.KeyDictionary["MoveRight"], Color.White, type);
			Content.Add(right);
			above = new KeyboardButton(new CPos(1536, -1024 + 2560, 1), Settings.KeyDictionary["MoveAbove"], Color.White, type);
			Content.Add(above);
			below = new KeyboardButton(new CPos(1536, -1024 + 3200, 0), Settings.KeyDictionary["MoveBelow"], Color.White, type);
			Content.Add(below);

			var tCam = new TextLine(new CPos(-3072, 3072, 0), FontManager.Pixel16, TextLine.OffsetType.RIGHT);
			tCam.SetText("Camera");
			Content.Add(tCam);
			line = 0;
			foreach (var dir in new[] { "Up", "Down", "Left", "Right" })
			{
				var text = new TextLine(new CPos(-1024, 3072 + line * 640, 0), font, TextLine.OffsetType.RIGHT);
				text.SetText(dir);
				line++;
				Content.Add(text);
			}
			camUp = new KeyboardButton(new CPos(1536, 3072, 0), Settings.KeyDictionary["CameraUp"], Color.White, type);
			Content.Add(camUp);
			camDown = new KeyboardButton(new CPos(1536, 3072 + 640, 1), Settings.KeyDictionary["CameraDown"], Color.White, type);
			Content.Add(camDown);
			camLeft = new KeyboardButton(new CPos(1536, 3072 + 1280, 1), Settings.KeyDictionary["CameraLeft"], Color.White, type);
			Content.Add(camLeft);
			camRight = new KeyboardButton(new CPos(1536, 3072 + 1920, 1), Settings.KeyDictionary["CameraRight"], Color.White, type);
			Content.Add(camRight);
		}

		public override void Hide()
		{
			base.Hide();
			save();
		}

		void save()
		{
			Settings.KeyDictionary.Clear();
			Settings.KeyDictionary.Add("Pause", pause.KeyString + "");
			Settings.KeyDictionary.Add("CameraLock", @lock.KeyString + "");
			Settings.KeyDictionary.Add("MoveUp", up.KeyString + "");
			Settings.KeyDictionary.Add("MoveDown", down.KeyString + "");
			Settings.KeyDictionary.Add("MoveLeft", left.KeyString + "");
			Settings.KeyDictionary.Add("MoveRight", right.KeyString + "");
			Settings.KeyDictionary.Add("MoveAbove", above.KeyString + "");
			Settings.KeyDictionary.Add("MoveBelow", below.KeyString + "");
			Settings.KeyDictionary.Add("CameraUp", camUp.KeyString + "");
			Settings.KeyDictionary.Add("CameraDown", camDown.KeyString + "");
			Settings.KeyDictionary.Add("CameraLeft", camLeft.KeyString + "");
			Settings.KeyDictionary.Add("CameraRight", camRight.KeyString + "");
			Settings.Save();

			game.AddInfoMessage(150, "Controls Saved!");
		}

		public override void Tick()
		{
			base.Tick();

			if (KeyInput.IsKeyDown("escape", 10))
				game.ChangeScreen(ScreenType.SETTINGS);
		}
	}
}
