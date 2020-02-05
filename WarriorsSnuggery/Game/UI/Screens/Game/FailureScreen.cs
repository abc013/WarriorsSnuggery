using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI
{
	public class FailureScreen : Screen
	{
		public FailureScreen(Game game) : base("Level Failed.")
		{
			Title.SetColor(Color.Red);
			Speed = 64;

			var score = new TextLine(new CPos(0, 1024, 0), Font.Pixel16, TextLine.OffsetType.MIDDLE);
			score.WriteText("Achieved Score: " + Color.Blue + game.Statistics.CalculateScore());
			Content.Add(score);
			var deaths = new TextLine(new CPos(0, 2048, 0), Font.Pixel16, TextLine.OffsetType.MIDDLE);
			deaths.WriteText(Color.Red + "Deaths: " + game.OldStatistics.Deaths);
			Content.Add(deaths);

			Content.Add(ButtonCreator.Create("wooden", new CPos(-2048, 5120, 0), "Restart Map", () => GameController.CreateNew(game.OldStatistics, sameSeed: true)));

			if (game.Type == GameType.TEST)
				Content.Add(ButtonCreator.Create("wooden", new CPos(2048, 5120, 0), "Main Menu", () => GameController.CreateNew(game.OldStatistics, GameType.MAINMENU)));
			else
				Content.Add(ButtonCreator.Create("wooden", new CPos(2048, 5120, 0), "Menu", () => GameController.CreateNew(game.OldStatistics, GameType.MENU)));
		}
	}
}
