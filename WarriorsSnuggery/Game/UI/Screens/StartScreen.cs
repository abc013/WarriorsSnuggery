using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI
{
	public class StartScreen : Screen
	{
		public StartScreen(Game game) : base("Welcome!")
		{
			Title.Position = new CPos(0, -4096, 0);

			Content.Add(ButtonCreator.Create("wooden", new CPos(4096, 6144, 0), "Okay", () => { game.Pause(false); game.ChangeScreen(ScreenType.DEFAULT); }));
			Content.Add(ButtonCreator.Create("wooden", new CPos(-4096, 6144, 0), "Exit", Program.Exit));

			var warning = new TextLine(new CPos(0, 5500, 0), IFont.Pixel16, TextLine.OffsetType.MIDDLE);
			warning.WriteText(new Color(0.5f, 0.5f, 1f) + "WS is still under development. If you encounter any bugs, please report them. Thanks for playing!");
			Content.Add(warning);

			var move = new TextLine(new CPos(0, 3072, 0), IFont.Pixel16, TextLine.OffsetType.MIDDLE);
			move.WriteText("Move: " + Color.Yellow + "W, A, S, D");
			Content.Add(move);

			var attack = new TextLine(new CPos(0, 2048, 0), IFont.Pixel16, TextLine.OffsetType.MIDDLE);
			attack.WriteText("Attack: " + Color.Yellow + "Left mouse button");
			Content.Add(attack);

			var aim = new TextLine(CPos.Zero, IFont.Pixel16, TextLine.OffsetType.MIDDLE);
			aim.SetText("Go up, and you'll get to the story mode. Walk down for the tutorial.");
			Content.Add(aim);

			var how = new TextLine(new CPos(0, 1024, 0), IFont.Pixel16, TextLine.OffsetType.MIDDLE);
			how.SetText("");
			Content.Add(how);

			var  @switch = new TextLine(new CPos(0, 4096, 0), IFont.Pixel16, TextLine.OffsetType.MIDDLE);
			@switch.WriteText("Select spells: " + Color.Cyan + "Mouse scroll");
			Content.Add(@switch);
		}
	}
}
