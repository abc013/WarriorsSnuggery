using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI
{
	public class FailureScreen : Screen
	{
		public FailureScreen(Game game) : base("Level Failed.")
		{
			Title.SetColor(Color.Red);

			var score = new TextLine(new CPos(0, 1024, 0), Font.Pixel16, TextLine.OffsetType.MIDDLE);
			score.WriteText("Achieved Score: " + Color.Blue + game.Statistics.CalculateScore());
			Content.Add(score);
			var deaths = new TextLine(new CPos(0, 2048, 0), Font.Pixel16, TextLine.OffsetType.MIDDLE);
			deaths.WriteText(Color.Red + "Deaths: " + game.OldStatistics.Deaths);
			Content.Add(deaths);

			if (game.Statistics.Hardcore)
			{
				game.Statistics.Save(game.World);
				game.Statistics.Delete();
				Content.Add(ButtonCreator.Create("wooden", new CPos(0, 5120, 0), "Return to Main Menu", () => GameController.CreateReturn(GameType.MAINMENU)));
			}
			else
			{
				Content.Add(ButtonCreator.Create("wooden", new CPos(-2048, 5120, 0), "Restart Map", () => GameController.CreateRestart()));

				if (game.Type == GameType.TEST)
					Content.Add(ButtonCreator.Create("wooden", new CPos(2048, 5120, 0), "Main Menu", () => GameController.CreateReturn(GameType.MAINMENU)));
				else
					Content.Add(ButtonCreator.Create("wooden", new CPos(2048, 5120, 0), "Menu", () => GameController.CreateReturn(GameType.MENU)));
			}
		}
	}
}
