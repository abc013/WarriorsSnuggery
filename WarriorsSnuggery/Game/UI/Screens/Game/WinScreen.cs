using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI
{
	class WinScreen : Screen
	{
		public WinScreen(Game game) : base("Congratulations!")
		{
			Title.Position = new CPos(0, -2048, 0);

			var won = new TextLine(new CPos(0, 0, 0), IFont.Pixel16, TextLine.OffsetType.MIDDLE);
			won.WriteText(Color.Blue + "Y" + Color.Yellow + "O" + Color.Green + "U " + Color.Magenta + "W" + Color.Blue + "O" + Color.Cyan + "N" + Color.Green + "!");
			Content.Add(won);
			var score = new TextLine(new CPos(0, 1024, 0), IFont.Pixel16, TextLine.OffsetType.MIDDLE);
			score.WriteText("Score: " + Color.Cyan + game.Statistics.CalculateScore());
			Content.Add(score);

			Content.Add(ButtonCreator.Create("wooden", new CPos(-2048, 5120, 0), "Headquarters", () => GameController.CreateNew(game.Statistics, GameType.MENU)));
			Content.Add(ButtonCreator.Create("wooden", new CPos(2048, 5120, 0), "Next Level", () => GameController.CreateNew(game.Statistics, GameType.NORMAL)));
		}
	}
}
