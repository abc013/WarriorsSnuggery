using WarriorsSnuggery.Objects;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.UI
{
	public class PausedScreen : Screen
	{
		readonly TextLine paused;
		readonly Game game;
		public PausedScreen(Game game) : base("Paused")
		{
			this.game = game;
			paused = new TextLine(new CPos(0,2048, 0), IFont.Pixel16, TextLine.OffsetType.MIDDLE);
			paused.WriteText(new Color(128, 128, 255) + "To unpause, press '" + Color.Yellow + "P" + new Color(128, 128, 255) + "'");
		}

		public override void Render()
		{
			base.Render();

			paused.Render();
		}

		public override void Tick()
		{
			base.Tick();

			paused.Tick();

			if(KeyInput.IsKeyDown("p", 10))
			{
				game.Pause(false);
				game.ChangeScreen(ScreenType.DEFAULT);
			}
		}

		public override void Dispose()
		{
			base.Dispose();

			paused.Dispose();
		}
	}
}
