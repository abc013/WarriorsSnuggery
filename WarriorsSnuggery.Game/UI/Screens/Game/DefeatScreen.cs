using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI.Screens
{
	public class DefeatScreen : Screen
	{
		public DefeatScreen(Game game) : base("Defeat.")
		{
			Title.SetColor(Color.Red);

			var score = new UITextLine(new CPos(0, 1024, 0), FontManager.Pixel16, TextOffset.MIDDLE);
			score.WriteText("Achieved Score: " + Color.Blue + game.Statistics.CalculateScore());
			Content.Add(score);
			var deaths = new UITextLine(new CPos(0, 2048, 0), FontManager.Pixel16, TextOffset.MIDDLE);
			deaths.WriteText(Color.Red + "Deaths: " + game.OldStatistics.Deaths);
			Content.Add(deaths);

			if (game.Statistics.Hardcore)
			{
				game.Statistics.Delete();
				Content.Add(new Button("Return to Main Menu", "wooden", GameController.CreateMainMenu) { Position = new CPos(0, 5120, 0) });
			}
			else
			{
				Content.Add(new Button("Restart Map", "wooden", GameController.CreateRestart) { Position = new CPos(-2048, 5120, 0) });

				switch (game.MissionType)
				{
					case MissionType.TEST:
						Content.Add(new Button("Main Menu", "wooden", GameController.CreateMainMenu) { Position = new CPos(2048, 5120, 0) });
						break;
					case MissionType.STORY:
					case MissionType.NORMAL:
					case MissionType.TUTORIAL:
						Content.Add(new Button("Menu", "wooden", GameController.CreateMenu) { Position = new CPos(2048, 5120, 0) });
						break;
				}
			}
		}
	}
}
