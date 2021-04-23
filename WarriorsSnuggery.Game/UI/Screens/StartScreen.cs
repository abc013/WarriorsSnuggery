using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;
using WarriorsSnuggery.UI.Objects;

namespace WarriorsSnuggery.UI.Screens
{
	public class StartScreen : Screen
	{
		public StartScreen(Game game) : base("")
		{
			var ws = new UIImage(new BatchObject(UISpriteManager.Get("logo")[0]))
			{
				Position = new CPos(0, -3072, 0),
				Scale = 0.8f
			};
			Add(ws);

			Add(new Button("Okay", "wooden", () => game.ShowScreen(ScreenType.DEFAULT, false)) { Position = new CPos(4096, 6144, 0) });
			Add(new Button("Exit", "wooden", Program.Exit) { Position = new CPos(-4096, 6144, 0) });

			var warning = new UITextBlock(FontManager.Pixel16, TextOffset.MIDDLE, new Color(0.5f, 0.5f, 1f) + "WS is still under development. If you encounter any bugs, please report them.", new Color(0.5f, 0.5f, 1f) + "Thank you!") { Position = new CPos(0, 4096, 0) };
			Add(warning);

			var @switch = new UITextLine(FontManager.Pixel16, TextOffset.MIDDLE) { Position = new CPos(0, 3072, 0) };
			@switch.WriteText("Select spells: " + Color.Cyan + "Mouse scroll");
			Add(@switch);

			var move = new UITextLine(FontManager.Pixel16, TextOffset.MIDDLE) { Position = new CPos(0, 2048, 0) };
			move.WriteText("Move: " + Color.Yellow + "W, A, S, D");
			Add(move);

			var attack = new UITextLine(FontManager.Pixel16, TextOffset.MIDDLE) { Position = new CPos(0, 1024, 0) };
			attack.WriteText("Attack: " + Color.Yellow + "Left mouse button");
			Add(attack);

			var aim2 = new UITextLine(FontManager.Pixel16, TextOffset.MIDDLE);
			aim2.SetText("To start the real game, move up!");
			Add(aim2);

			var aim = new UITextLine(FontManager.Pixel16, TextOffset.MIDDLE) { Position = new CPos(0, -1024, 0) };
			aim.SetText("For the tutorial, please move down.");
			Add(aim);
		}
	}
}
