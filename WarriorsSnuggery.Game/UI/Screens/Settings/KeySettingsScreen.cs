using OpenTK.Windowing.GraphicsLibraryFramework;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;
using WarriorsSnuggery.UI.Objects;

namespace WarriorsSnuggery.UI.Screens
{
	class KeySettingsScreen : Screen
	{
		readonly Game game;

		readonly KeyboardButton up, down, right, left, above, below, pause, camUp, camDown, camRight, camLeft, @lock, activate;

		public KeySettingsScreen(Game game) : base("Key Bindings")
		{
			this.game = game;

			Title.Position = new CPos(0, -4096, 0);

			var font = FontManager.Default;

			Add(new Button("Save & Back", "wooden", () => game.ShowScreen(ScreenType.SETTINGS)) { Position = new CPos(0, 6144, 0) });

			var type = PanelCache.Types["wooden"];

			var tPause = new UITextLine(font, TextOffset.RIGHT) { Position = new CPos(-1024, -4096, 0) };
			tPause.SetText("Pause/unpause");
			Add(tPause);
			pause = new KeyboardButton(Settings.KeyDictionary["Pause"], type) { Position = new CPos(1536, -4096, 0) };
			Add(pause);

			var tLock = new UITextLine(font, TextOffset.RIGHT) { Position = new CPos(-1024, -3072, 0) };
			tLock.SetText("Toggle camera lock");
			Add(tLock);
			@lock = new KeyboardButton(Settings.KeyDictionary["CameraLock"], type) { Position = new CPos(1536, -3072, 0) };
			Add(@lock);

			var tactivate = new UITextLine(font, TextOffset.RIGHT) { Position = new CPos(-1024, -2048, 0) };
			tactivate.SetText("Activate spell/actor");
			Add(tactivate);
			activate = new KeyboardButton(Settings.KeyDictionary["Activate"], type) { Position = new CPos(1536, -2048, 0) };
			Add(activate);

			var tMove = new UITextLine(font, TextOffset.RIGHT) { Position = new CPos(-3072, -1024, 0) };
			tMove.SetText("Movement");
			Add(tMove);
			var line = 0;
			foreach (var dir in new[] { "Up", "Down", "Left", "Right", "Above", "Below" })
			{
				var text = new UITextLine(font, TextOffset.RIGHT) { Position = new CPos(-1024, -1024 + line * 640, 0) };
				text.SetText(dir);
				line++;
				Add(text);
			}
			up = new KeyboardButton(Settings.KeyDictionary["MoveUp"], type) { Position = new CPos(1536, -1024, 0) };
			Add(up);
			down = new KeyboardButton(Settings.KeyDictionary["MoveDown"], type) { Position = new CPos(1536, -1024 + 640, 1) };
			Add(down);
			left = new KeyboardButton(Settings.KeyDictionary["MoveLeft"], type) { Position = new CPos(1536, -1024 + 1280, 1) };
			Add(left);
			right = new KeyboardButton(Settings.KeyDictionary["MoveRight"], type) { Position = new CPos(1536, -1024 + 1920, 0) };
			Add(right);
			above = new KeyboardButton(Settings.KeyDictionary["MoveAbove"], type) { Position = new CPos(1536, -1024 + 2560, 1) };
			Add(above);
			below = new KeyboardButton(Settings.KeyDictionary["MoveBelow"], type) { Position = new CPos(1536, -1024 + 3200, 0) };
			Add(below);

			var tCam = new UITextLine(font, TextOffset.RIGHT) { Position = new CPos(-3072, 3072, 0) };
			tCam.SetText("Camera");
			Add(tCam);
			line = 0;
			foreach (var dir in new[] { "Up", "Down", "Left", "Right" })
			{
				var text = new UITextLine(font, TextOffset.RIGHT) { Position = new CPos(-1024, 3072 + line * 640, 0) };
				text.SetText(dir);
				line++;
				Add(text);
			}
			camUp = new KeyboardButton(Settings.KeyDictionary["CameraUp"], type) { Position = new CPos(1536, 3072, 0) };
			Add(camUp);
			camDown = new KeyboardButton(Settings.KeyDictionary["CameraDown"], type) { Position = new CPos(1536, 3072 + 640, 1) };
			Add(camDown);
			camLeft = new KeyboardButton(Settings.KeyDictionary["CameraLeft"], type) { Position = new CPos(1536, 3072 + 1280, 1) };
			Add(camLeft);
			camRight = new KeyboardButton(Settings.KeyDictionary["CameraRight"], type) { Position = new CPos(1536, 3072 + 1920, 1) };
			Add(camRight);
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
			Settings.KeyDictionary.Add("Activate", activate.Key);
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
			Log.Debug("Saved key bindings.");
		}

		public override void KeyDown(Keys key, bool isControl, bool isShift, bool isAlt)
		{
			base.KeyDown(key, isControl, isShift, isAlt);

			if (key == Keys.Escape)
				game.ShowScreen(ScreenType.SETTINGS);
		}
	}
}
