using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI
{
	public class PausedScreen : Screen
	{
		readonly Game game;
		public PausedScreen(Game game) : base("Paused")
		{
			this.game = game;
			var paused = new TextLine(new CPos(0, 2048, 0), IFont.Pixel16, TextLine.OffsetType.MIDDLE);
			paused.WriteText(new Color(128, 128, 255) + "To unpause, press '" + Color.Yellow + "P" + new Color(128, 128, 255) + "'");
			Content.Add(paused);
		}

		public override void Tick()
		{
			base.Tick();

			if (KeyInput.IsKeyDown("p", 10))
			{
				game.Pause(false);
				game.ChangeScreen(ScreenType.DEFAULT);
			}
		}
	}
}
