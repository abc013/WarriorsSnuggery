using OpenTK.Windowing.GraphicsLibraryFramework;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.UI.Objects;

namespace WarriorsSnuggery.UI.Screens
{
	class KeySettingsScreen : Screen
	{
		readonly Game game;

		readonly KeyboardButton up, down, right, left, above, below, pause, camUp, camDown, camRight, camLeft, @lock, activate;

		public KeySettingsScreen(Game game) : base("")
		{
			this.game = game;
			Title.Position = new UIPos(0, -4096);
			Add(new SettingsChooser(game, new UIPos(0, -5120), ScreenType.KEYSETTINGS, save));

			var font = FontManager.Default;
			var type = PanelCache.Types["wooden"];

			var tPause = new UIText(font, TextOffset.RIGHT) { Position = new UIPos(-1024, -4096) };
			tPause.SetText("Pause/unpause");
			Add(tPause);
			pause = new KeyboardButton(Settings.GetKey("Pause"), type) { Position = new UIPos(1536, -4096) };
			Add(pause);

			var tLock = new UIText(font, TextOffset.RIGHT) { Position = new UIPos(-1024, -3072) };
			tLock.SetText("Toggle camera lock");
			Add(tLock);
			@lock = new KeyboardButton(Settings.GetKey("CameraLock"), type) { Position = new UIPos(1536, -3072) };
			Add(@lock);

			var tactivate = new UIText(font, TextOffset.RIGHT) { Position = new UIPos(-1024, -2048) };
			tactivate.SetText("Activate spell/actor");
			Add(tactivate);
			activate = new KeyboardButton(Settings.GetKey("Activate"), type) { Position = new UIPos(1536, -2048) };
			Add(activate);

			var tMove = new UIText(font, TextOffset.RIGHT) { Position = new UIPos(-3072, -1024) };
			tMove.SetText("Movement");
			Add(tMove);
			var line = 0;
			foreach (var dir in new[] { "Up", "Down", "Left", "Right", "Above", "Below" })
			{
				var text = new UIText(font, TextOffset.RIGHT) { Position = new UIPos(-1024, -1024 + line * 640) };
				text.SetText(dir);
				line++;
				Add(text);
			}
			up = new KeyboardButton(Settings.GetKey("MoveUp"), type) { Position = new UIPos(1536, -1024) };
			Add(up);
			down = new KeyboardButton(Settings.GetKey("MoveDown"), type) { Position = new UIPos(1536, -1024 + 640) };
			Add(down);
			left = new KeyboardButton(Settings.GetKey("MoveLeft"), type) { Position = new UIPos(1536, -1024 + 1280) };
			Add(left);
			right = new KeyboardButton(Settings.GetKey("MoveRight"), type) { Position = new UIPos(1536, -1024 + 1920) };
			Add(right);
			above = new KeyboardButton(Settings.GetKey("MoveAbove"), type) { Position = new UIPos(1536, -1024 + 2560) };
			Add(above);
			below = new KeyboardButton(Settings.GetKey("MoveBelow"), type) { Position = new UIPos(1536, -1024 + 3200) };
			Add(below);

			var tCam = new UIText(font, TextOffset.RIGHT) { Position = new UIPos(-3072, 3072) };
			tCam.SetText("Camera");
			Add(tCam);
			line = 0;
			foreach (var dir in new[] { "Up", "Down", "Left", "Right" })
			{
				var text = new UIText(font, TextOffset.RIGHT) { Position = new UIPos(-1024, 3072 + line * 640) };
				text.SetText(dir);
				line++;
				Add(text);
			}
			camUp = new KeyboardButton(Settings.GetKey("CameraUp"), type) { Position = new UIPos(1536, 3072) };
			Add(camUp);
			camDown = new KeyboardButton(Settings.GetKey("CameraDown"), type) { Position = new UIPos(1536, 3072 + 640) };
			Add(camDown);
			camLeft = new KeyboardButton(Settings.GetKey("CameraLeft"), type) { Position = new UIPos(1536, 3072 + 1280) };
			Add(camLeft);
			camRight = new KeyboardButton(Settings.GetKey("CameraRight"), type) { Position = new UIPos(1536, 3072 + 1920) };
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
				game.ShowScreen(ScreenType.MENU);
		}
	}
}
