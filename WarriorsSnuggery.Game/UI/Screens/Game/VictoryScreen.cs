using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;
using WarriorsSnuggery.UI.Objects;

namespace WarriorsSnuggery.UI.Screens
{
	class VictoryScreen : Screen
	{
		public VictoryScreen(Game game) : base("Victory!")
		{
			Title.Position = new CPos(0, -2048, 0);

			var won = new UITextLine(FontManager.Default, TextOffset.MIDDLE);
			won.WriteText("Level has been successfully completed. Congratulations!");
			Add(won);
			var score = new UITextLine(FontManager.Default, TextOffset.MIDDLE) { Position = new CPos(0, 1024, 0) };
			score.WriteText("Score: " + Color.Cyan + game.Save.CalculateScore());
			Add(score);

			Add(new Button("Headquarters", "wooden", GameController.CreateNextMenu) { Position = new CPos(-2048, 5120, 0) });
			Add(new Button("Next Level", "wooden", GameController.CreateNext) { Position = new CPos(2048, 5120, 0) });
		}
	}
}
