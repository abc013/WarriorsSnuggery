using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI.Screens
{
	class VictoryScreen : Screen
	{
		public VictoryScreen(Game game) : base("Level Cleared.")
		{
			Title.Position = new CPos(0, -2048, 0);

			var won = new UITextLine(new CPos(0, 0, 0), FontManager.Pixel16, TextOffset.MIDDLE);
			won.WriteText("Level has been successfully cleared from any enemy opposition.");
			Content.Add(won);
			var score = new UITextLine(new CPos(0, 1024, 0), FontManager.Pixel16, TextOffset.MIDDLE);
			score.WriteText("Score: " + Color.Cyan + game.Statistics.CalculateScore());
			Content.Add(score);

			Content.Add(new Button(new CPos(-2048, 5120, 0), "Headquarters", "wooden", () => GameController.CreateNext(GameType.MENU)));
			Content.Add(new Button(new CPos(2048, 5120, 0), "Next Level", "wooden", () => GameController.CreateNext(GameType.NORMAL)));
		}
	}
}
