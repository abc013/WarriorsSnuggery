using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI
{
	class WinScreen : Screen
	{
		readonly Button menu;
		readonly Button next;
		readonly TextLine score;
		readonly TextLine won;
		readonly Game game;
		bool firsttick = true;

		public WinScreen(Game game) : base("Congratulations!")
		{
			this.game = game;
			Title.Position = new CPos(0, -2048, 0);

			won = new TextLine(new CPos(0, 0, 0), IFont.Pixel16, TextLine.OffsetType.MIDDLE);
			won.WriteText(Color.Blue + "Y" + Color.Yellow + "O" + Color.Green + "U " + Color.Magenta + "W" + Color.Blue + "O" + Color.Cyan + "N" + Color.Green + "!");
			score = new TextLine(new CPos(0, 1024, 0), IFont.Pixel16, TextLine.OffsetType.MIDDLE);

			menu = ButtonCreator.Create("wooden", new CPos(-2048, 5120, 0), "Headquarters", () => Window.Current.NewGame(game.Statistics, GameType.MENU));
			next = ButtonCreator.Create("wooden", new CPos(2048, 5120, 0), "Next Level", () => Window.Current.NewGame(game.Statistics, GameType.NORMAL));
		}

		public override void Render()
		{
			base.Render();

			won.Render();
			score.Render();

			menu.Render();
			next.Render();
		}

		public override void Tick()
		{
			base.Tick();

			if (firsttick)
			{
				firsttick = false;
				score.WriteText("Score: " + Color.Cyan + game.Statistics.CalculateScore());
			}

			won.Tick();
			score.Tick();

			menu.Tick();
			next.Tick();
		}

		public override void Dispose()
		{
			base.Dispose();

			won.Dispose();
			score.Dispose();

			menu.Dispose();
			next.Dispose();
		}
	}
}
