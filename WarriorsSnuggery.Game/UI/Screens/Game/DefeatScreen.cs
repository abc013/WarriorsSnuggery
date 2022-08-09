using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.UI.Objects;

namespace WarriorsSnuggery.UI.Screens
{
	public class DefeatScreen : Screen
	{
		public DefeatScreen(Game game) : base("Defeat.")
		{
			Title.Color = Color.Red;

			var score = new UIText(FontManager.Default, TextOffset.MIDDLE) { Position = new UIPos(0, 1024) };
			score.SetText("Achieved Score: " + Color.Blue + game.Save.CalculateScore());
			Add(score);
			var deaths = new UIText(FontManager.Default, TextOffset.MIDDLE) { Position = new UIPos(0, 2048) };
			deaths.SetText(Color.Red + "Deaths: " + game.Save.Deaths);
			Add(deaths);

			if (game.Save.Hardcore)
			{
				game.Save.Delete();
				Add(new Button("Return to Main Menu", "wooden", GameController.CreateMainMenu) { Position = new UIPos(0, 5120) });
			}
			else
			{
				Add(new Button("Restart Map", "wooden", GameController.CreateRestart) { Position = new UIPos(-2048, 5120) });

				switch (game.MissionType)
				{
					case MissionType.TEST:
						Add(new Button("Main Menu", "wooden", GameController.CreateMainMenu) { Position = new UIPos(2048, 5120) });
						break;
					case MissionType.STORY:
					case MissionType.NORMAL:
					case MissionType.TUTORIAL:
						Add(new Button("Menu", "wooden", GameController.CreateMenu) { Position = new UIPos(2048, 5120) });
						break;
				}
			}
		}
	}
}
