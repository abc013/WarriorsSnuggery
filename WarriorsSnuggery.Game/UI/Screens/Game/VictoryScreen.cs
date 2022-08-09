using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.UI.Objects;

namespace WarriorsSnuggery.UI.Screens
{
	class VictoryScreen : Screen
	{
		public VictoryScreen(Game game) : base("Victory!")
		{
			Title.Position = new UIPos(0, -2048);

			var won = new UIText(FontManager.Default, TextOffset.MIDDLE);
			won.SetText("Level has been successfully completed. Congratulations!");
			Add(won);
			var score = new UIText(FontManager.Default, TextOffset.MIDDLE) { Position = new UIPos(0, 1024) };
			score.SetText("Score: " + Color.Cyan + game.Save.CalculateScore());
			Add(score);

			Add(new Button("Headquarters", "wooden", GameController.CreateNextMenu) { Position = new UIPos(-2048, 5120) });
			Add(new Button("Next Level", "wooden", GameController.CreateNext) { Position = new UIPos(2048, 5120) });
		}
	}
}
