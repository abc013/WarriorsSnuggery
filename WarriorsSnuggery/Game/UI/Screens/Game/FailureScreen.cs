/*
 * User: Andreas
 * Date: 11.07.2018
 * Time: 18:59
 */
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI
{
	public class FailureScreen : Screen
	{
		readonly Button restart;
		readonly Button menu;
		readonly TextLine score;
		readonly TextLine deaths;
		readonly Game game;
		bool firsttick = true;

		public FailureScreen(Game game) : base("You Failed.")
		{
			this.game = game;
			Title.SetColor(Color.Red);
			Speed = 64;

			score = new TextLine(new CPos(0, 1024, 0), IFont.Pixel16, TextLine.OffsetType.MIDDLE);
			deaths = new TextLine(new CPos(0, 2048, 0), IFont.Pixel16, TextLine.OffsetType.MIDDLE);

			restart = ButtonCreator.Create("wooden", new CPos(-2048, 5120, 0), "Restart Map", () => GameController.CreateNew(game.OldStatistics, sameSeed: true));

			if (game.Type == GameType.TEST)
				menu = ButtonCreator.Create("wooden", new CPos(2048, 5120, 0), "Main Menu", () => GameController.CreateNew(game.OldStatistics, GameType.MAINMENU));
			else
				menu = ButtonCreator.Create("wooden", new CPos(2048, 5120, 0), "Menu", () => GameController.CreateNew(game.OldStatistics, GameType.MENU));
		}

		public override void Render()
		{
			base.Render();

			restart.Render();
			menu.Render();
			score.Render();
			deaths.Render();
		}

		public override void Tick()
		{
			base.Tick();

			if (firsttick)
			{
				firsttick = false;
				score.WriteText("Achieved Score: " + Color.Blue + game.Statistics.CalculateScore());
				deaths.WriteText(Color.Red + "Deaths: " + game.OldStatistics.Deaths);
			}

			restart.Tick();
			menu.Tick();
			score.Tick();
			deaths.Tick();
		}

		public override void Dispose()
		{
			base.Dispose();

			restart.Dispose();
			menu.Dispose();
			score.Dispose();
			deaths.Dispose();
		}
	}
}
