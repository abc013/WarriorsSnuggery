using OpenTK.Windowing.GraphicsLibraryFramework;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI.Screens
{
	public class PausedScreen : Screen
	{
		readonly Game game;
		public PausedScreen(Game game) : base("Paused")
		{
			this.game = game;
			var paused = new UITextLine(FontManager.Pixel16, TextOffset.MIDDLE) { Position = new CPos(0, 2048, 0) };
			paused.WriteText(new Color(128, 128, 255) + "To unpause, press '" + Color.Yellow + Settings.GetKey("Pause") + new Color(128, 128, 255) + "'");
			Add(paused);
		}

		public override void KeyDown(Keys key, bool isControl, bool isShift, bool isAlt)
		{
			base.KeyDown(key, isControl, isShift, isAlt);

			if (key == Settings.GetKey("Pause"))
				game.ShowScreen(ScreenType.DEFAULT, false);
		}
	}
}
