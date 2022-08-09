using OpenTK.Windowing.GraphicsLibraryFramework;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.UI.Screens
{
	public class PausedScreen : Screen
	{
		readonly Game game;
		public PausedScreen(Game game) : base("Paused")
		{
			this.game = game;
			var paused = new UIText(FontManager.Default, TextOffset.MIDDLE) { Position = new UIPos(0, 2048) };
			paused.SetText(new Color(128, 128, 255) + "To unpause, press '" + Color.Yellow + Settings.GetKey("Pause") + new Color(128, 128, 255) + "'");
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
