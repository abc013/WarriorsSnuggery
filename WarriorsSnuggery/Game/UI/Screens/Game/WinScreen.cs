using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI
{
	class WinScreen : Screen
	{
		public WinScreen(Game game) : base("Level Cleared.")
		{
			Title.Position = new CPos(0, -2048, 0);

			var won = new TextLine(new CPos(0, 0, 0), FontManager.Pixel16, TextLine.OffsetType.MIDDLE);
			won.WriteText("Level has been successfully cleared from any enemy opposition.");
			Content.Add(won);
			var score = new TextLine(new CPos(0, 1024, 0), FontManager.Pixel16, TextLine.OffsetType.MIDDLE);
			score.WriteText("Score: " + Color.Cyan + game.Statistics.CalculateScore());
			Content.Add(score);

			Content.Add(ButtonCreator.Create("wooden", new CPos(-2048, 5120, 0), "Headquarters", () => GameController.CreateNext(GameType.MENU)));
			Content.Add(ButtonCreator.Create("wooden", new CPos(2048, 5120, 0), "Next Level", () => GameController.CreateNext(GameType.NORMAL)));
		}
	}
}
