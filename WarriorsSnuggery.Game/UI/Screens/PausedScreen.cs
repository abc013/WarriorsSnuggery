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
			var paused = new UITextLine(new CPos(0, 2048, 0), FontManager.Pixel16, TextOffset.MIDDLE);
			paused.WriteText(new Color(128, 128, 255) + "To unpause, press '" + Color.Yellow + "P" + new Color(128, 128, 255) + "'");
			Content.Add(paused);
		}

		public override void KeyDown(Keys key, bool isControl, bool isShift, bool isAlt)
		{
			if (key == Keys.P)
				game.ShowScreen(ScreenType.DEFAULT, false);
		}
	}
}
