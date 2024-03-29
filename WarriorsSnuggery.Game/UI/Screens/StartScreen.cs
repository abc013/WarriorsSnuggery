using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.UI.Objects;

namespace WarriorsSnuggery.UI.Screens
{
	public class StartScreen : Screen
	{
		public StartScreen(Game game) : base("")
		{
			var ws = new UIImage(new BatchObject(UISpriteManager.Get("logo")[0]))
			{
				Position = new UIPos(0, -3072),
				Scale = 0.8f
			};
			Add(ws);

			var welcome = new UIText(FontManager.Header, TextOffset.MIDDLE) { Position = new UIPos(0, -512) };
			welcome.SetText("Welcome to Warrior's Snuggery!");
			Add(welcome);

			var tutorial = new UIText(FontManager.Header, TextOffset.MIDDLE) { Position = new UIPos(0, 1536) };
			tutorial.SetText("If you don't know how to play yet, move down to the tutorial!");
			Add(tutorial);

			var warning = new UIText(FontManager.Header, TextOffset.MIDDLE) { Position = new UIPos(0, 4096), Color = new Color(0.5f, 0.5f, 1f) };
			warning.SetText("WS is still under development. If you encounter any bugs, please report them.");
			Add(warning);

			Add(new Button("Lets go!", "wooden", () => game.ShowScreen(ScreenType.DEFAULT, false)) { Position = new UIPos(0, 6144) });
		}
	}
}
