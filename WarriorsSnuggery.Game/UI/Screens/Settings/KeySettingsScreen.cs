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

			Content.Add(new Button(new CPos(0, 6144, 0), "Save & Back", "wooden", () => game.ShowScreen(ScreenType.SETTINGS)));

			var type = PanelManager.Get("wooden");

			var tPause = new UITextLine(new CPos(-1024, -3072, 0), FontManager.Pixel16, TextOffset.RIGHT);
			tPause.SetText("Pause/unpause");
			Content.Add(tPause);
			pause = new KeyboardButton(new CPos(1536, -3072, 0), Settings.KeyDictionary["Pause"], Color.White, type);
			Content.Add(pause);
			var tLock = new UITextLine(new CPos(-1024, -2048, 0), FontManager.Pixel16, TextOffset.RIGHT);
			tLock.SetText("Toggle camera lock");
			Content.Add(tLock);
			@lock = new KeyboardButton(new CPos(1536, -2048, 0), Settings.KeyDictionary["CameraLock"], Color.White, type);
			Content.Add(@lock);

			var font = FontManager.Pixel16;

			var tMove = new UITextLine(new CPos(-3072, -1024, 0), FontManager.Pixel16, TextOffset.RIGHT);
			tMove.SetText("Movement");
			Content.Add(tMove);
			var line = 0;
			foreach (var dir in new[] { "Up", "Down", "Left", "Right", "Above", "Below" })
			{
				var text = new UITextLine(new CPos(-1024, -1024 + line * 640, 0), font, TextOffset.RIGHT);
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

			var tCam = new UITextLine(new CPos(-3072, 3072, 0), FontManager.Pixel16, TextOffset.RIGHT);
			tCam.SetText("Camera");
			Content.Add(tCam);
			line = 0;
			foreach (var dir in new[] { "Up", "Down", "Left", "Right" })
			{
				var text = new UITextLine(new CPos(-1024, 3072 + line * 640, 0), font, TextOffset.RIGHT);
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
