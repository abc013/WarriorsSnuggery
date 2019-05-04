using WarriorsSnuggery.Objects;
using WarriorsSnuggery.Graphics;

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

			score = new TextLine(new CPos(0,1024,0), IFont.Pixel16, TextLine.OffsetType.MIDDLE);
			deaths = new TextLine(new CPos(0, 2048, 0), IFont.Pixel16, TextLine.OffsetType.MIDDLE);

			restart = ButtonCreator.Create("wooden", new CPos(-2048, 5120,0), "Restart Map", () => Window.Current.NewGame(game.OldStatistics, sameSeed: true));
			menu = game.Type == GameType.TEST ? ButtonCreator.Create("wooden", new CPos(2048, 5120, 0), "Main Menu", () => Window.Current.NewGame(game.OldStatistics, GameType.MAINMENU)) : ButtonCreator.Create("wooden", new CPos(2048, 5120, 0), "Menu", () => Window.Current.NewGame(game.OldStatistics, GameType.MENU));
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

			restart.Tick();
			menu.Tick();

			if (firsttick)
			{
				firsttick = false;
				score.WriteText("Score: " + Color.Blue + (game.Statistics.Level * game.Statistics.FinalLevel + game.Statistics.Mana * 3 - game.Statistics.Deaths * 7 + game.Statistics.Kills * 4));
				deaths.WriteText(Color.Red + "Deaths: " + game.Statistics.Deaths);
			}
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
