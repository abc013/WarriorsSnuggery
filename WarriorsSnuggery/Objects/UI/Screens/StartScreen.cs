using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI
{
	public class StartScreen : Screen
	{
		public StartScreen(Game game) : base("")
		{
			var ws = new UIImage(new CPos(0, -3072, 0), new BatchObject(UITextureManager.Get("logo")[0], Color.White), 0.8f);
			Content.Add(ws);

			Content.Add(new Button(new CPos(4096, 6144, 0), "Okay", "wooden", () => game.ChangeScreen(ScreenType.DEFAULT, false)));
			Content.Add(new Button(new CPos(-4096, 6144, 0), "Exit", "wooden", Program.Exit));

			var warning = new TextBlock(new CPos(0, 4096, 0), FontManager.Pixel16, TextLine.OffsetType.MIDDLE, new Color(0.5f, 0.5f, 1f) + "WS is still under development. If you encounter any bugs, please report them.", new Color(0.5f, 0.5f, 1f) + "Thank you!");
			Content.Add(warning);

			var @switch = new TextLine(new CPos(0, 3072, 0), FontManager.Pixel16, TextLine.OffsetType.MIDDLE);
			@switch.WriteText("Select spells: " + Color.Cyan + "Mouse scroll");
			Content.Add(@switch);

			var move = new TextLine(new CPos(0, 2048, 0), FontManager.Pixel16, TextLine.OffsetType.MIDDLE);
			move.WriteText("Move: " + Color.Yellow + "W, A, S, D");
			Content.Add(move);

			var attack = new TextLine(new CPos(0, 1024, 0), FontManager.Pixel16, TextLine.OffsetType.MIDDLE);
			attack.WriteText("Attack: " + Color.Yellow + "Left mouse button");
			Content.Add(attack);

			var aim2 = new TextLine(CPos.Zero, FontManager.Pixel16, TextLine.OffsetType.MIDDLE);
			aim2.SetText("To start the real game, move up!");
			Content.Add(aim2);

			var aim = new TextLine(new CPos(0, -1024, 0), FontManager.Pixel16, TextLine.OffsetType.MIDDLE);
			aim.SetText("For the tutorial, please move down.");
			Content.Add(aim);
		}
	}
}
