using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;
using WarriorsSnuggery.UI.Objects;

namespace WarriorsSnuggery.UI.Screens
{
	public class DefeatScreen : Screen
	{
		public DefeatScreen(Game game) : base("Defeat.")
		{
			Title.SetColor(Color.Red);

			var score = new UITextLine(FontManager.Pixel16, TextOffset.MIDDLE) { Position = new CPos(0, 1024, 0) };
			score.WriteText("Achieved Score: " + Color.Blue + game.Save.CalculateScore());
			Add(score);
			var deaths = new UITextLine(FontManager.Pixel16, TextOffset.MIDDLE) { Position = new CPos(0, 2048, 0) };
			deaths.WriteText(Color.Red + "Deaths: " + game.Save.Deaths);
			Add(deaths);

			if (game.Save.Hardcore)
			{
				game.Save.Delete();
				Add(new Button("Return to Main Menu", "wooden", GameController.CreateMainMenu) { Position = new CPos(0, 5120, 0) });
			}
			else
			{
				Add(new Button("Restart Map", "wooden", GameController.CreateRestart) { Position = new CPos(-2048, 5120, 0) });

				switch (game.MissionType)
				{
					case MissionType.TEST:
						Add(new Button("Main Menu", "wooden", GameController.CreateMainMenu) { Position = new CPos(2048, 5120, 0) });
						break;
					case MissionType.STORY:
					case MissionType.NORMAL:
					case MissionType.TUTORIAL:
						Add(new Button("Menu", "wooden", GameController.CreateMenu) { Position = new CPos(2048, 5120, 0) });
						break;
				}
			}
		}
	}
}
