using OpenTK.Windowing.GraphicsLibraryFramework;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI.Screens
{
	class KeySettingsScreen : Screen
	{
		readonly Game game;

		readonly KeyboardButton up, down, right, left, above, below, pause, camUp, camDown, camRight, camLeft, @lock;

		public KeySettingsScreen(Game game) : base("Key Bindings")
		{
			this.game = game;

			Title.Position = new CPos(0, -4096, 0);

			Content.Add(new Button("Save & Back", "wooden", () => game.ShowScreen(ScreenType.SETTINGS)) { Position = new CPos(0, 6144, 0) });

			var type = PanelManager.Get("wooden");

			var tPause = new UITextLine(FontManager.Pixel16, TextOffset.RIGHT) { Position = new CPos(-1024, -3072, 0) };
			tPause.SetText("Pause/unpause");
			Content.Add(tPause);
			pause = new KeyboardButton(Settings.KeyDictionary["Pause"], type) { Position = new CPos(1536, -3072, 0) };
			Content.Add(pause);
			var tLock = new UITextLine(FontManager.Pixel16, TextOffset.RIGHT) { Position = new CPos(-1024, -2048, 0) };
			tLock.SetText("Toggle camera lock");
			Content.Add(tLock);
			@lock = new KeyboardButton(Settings.KeyDictionary["CameraLock"], type) { Position = new CPos(1536, -2048, 0) };
			Content.Add(@lock);

			var font = FontManager.Pixel16;

			var tMove = new UITextLine(FontManager.Pixel16, TextOffset.RIGHT) { Position = new CPos(-3072, -1024, 0) };
			tMove.SetText("Movement");
			Content.Add(tMove);
			var line = 0;
			foreach (var dir in new[] { "Up", "Down", "Left", "Right", "Above", "Below" })
			{
				var text = new UITextLine(font, TextOffset.RIGHT) { Position = new CPos(-1024, -1024 + line * 640, 0) };
				text.SetText(dir);
				line++;
				Content.Add(text);
			}
			up = new KeyboardButton(Settings.KeyDictionary["MoveUp"], type) { Position = new CPos(1536, -1024, 0) };
			Content.Add(up);
			down = new KeyboardButton(Settings.KeyDictionary["MoveDown"], type) { Position = new CPos(1536, -1024 + 640, 1) };
			Content.Add(down);
			left = new KeyboardButton(Settings.KeyDictionary["MoveLeft"], type) { Position = new CPos(1536, -1024 + 1280, 1) };
			Content.Add(left);
			right = new KeyboardButton(Settings.KeyDictionary["MoveRight"], type) { Position = new CPos(1536, -1024 + 1920, 0) };
			Content.Add(right);
			above = new KeyboardButton(Settings.KeyDictionary["MoveAbove"], type) { Position = new CPos(1536, -1024 + 2560, 1) };
			Content.Add(above);
			below = new KeyboardButton(Settings.KeyDictionary["MoveBelow"], type) { Position = new CPos(1536, -1024 + 3200, 0) };
			Content.Add(below);

			var tCam = new UITextLine(FontManager.Pixel16, TextOffset.RIGHT) { Position = new CPos(-3072, 3072, 0) };
			tCam.SetText("Camera");
			Content.Add(tCam);
			line = 0;
			foreach (var dir in new[] { "Up", "Down", "Left", "Right" })
			{
				var text = new UITextLine(font, TextOffset.RIGHT) { Position = new CPos(-1024, 3072 + line * 640, 0) };
				text.SetText(dir);
				line++;
				Content.Add(text);
			}
			camUp = new KeyboardButton(Settings.KeyDictionary["CameraUp"], type) { Position = new CPos(1536, 3072, 0) };
			Content.Add(camUp);
			camDown = new KeyboardButton(Settings.KeyDictionary["CameraDown"], type) { Position = new CPos(1536, 3072 + 640, 1) };
			Content.Add(camDown);
			camLeft = new KeyboardButton(Settings.KeyDictionary["CameraLeft"], type) { Position = new CPos(1536, 3072 + 1280, 1) };
			Content.Add(camLeft);
			camRight = new KeyboardButton(Settings.KeyDictionary["CameraRight"], type) { Position = new CPos(1536, 3072 + 1920, 1) };
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
			Settings.KeyDictionary.Add("Pause", pause.Key);
			Settings.KeyDictionary.Add("CameraLock", @lock.Key);
			Settings.KeyDictionary.Add("MoveUp", up.Key);
			Settings.KeyDictionary.Add("MoveDown", down.Key);
			Settings.KeyDictionary.Add("MoveLeft", left.Key);
			Settings.KeyDictionary.Add("MoveRight", right.Key);
			Settings.KeyDictionary.Add("MoveAbove", above.Key);
			Settings.KeyDictionary.Add("MoveBelow", below.Key);
			Settings.KeyDictionary.Add("CameraUp", camUp.Key);
			Settings.KeyDictionary.Add("CameraDown", camDown.Key);
			Settings.KeyDictionary.Add("CameraLeft", camLeft.Key);
			Settings.KeyDictionary.Add("CameraRight", camRight.Key);
			Settings.Save();

			game.AddInfoMessage(150, "Controls Saved!");
			Log.WriteDebug("Saved key bindings.");
		}

		public override void KeyDown(Keys key, bool isControl, bool isShift, bool isAlt)
		{
			if (key == Keys.Escape)
				game.ShowScreen(ScreenType.SETTINGS);
		}
	}
}
