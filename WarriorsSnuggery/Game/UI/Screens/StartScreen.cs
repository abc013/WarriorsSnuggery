/*
 * User: Andreas
 * Date: 13.10.2018
 * Time: 19:34
 */
using WarriorsSnuggery.Objects;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.UI
{
	public class StartScreen : Screen
	{
		readonly Button back;
		readonly Button exit;

		readonly TextLine warning;
		readonly TextLine move;
		readonly TextLine attack;
		readonly TextLine @switch;
		readonly TextLine aim;
		readonly TextLine how;

		readonly Game game;

		public StartScreen(Game game) : base("Welcome!")
		{
			this.game = game;
			Title.Position = new CPos(0,-4096, 0);

			back = ButtonCreator.Create("wooden", new CPos(4096, 6144, 0), "Okay", game.Pause);
			exit = ButtonCreator.Create("wooden", new CPos(-4096, 6144, 0), "Exit", Window.Current.Exit);

			warning = new TextLine(new CPos(0, 5500, 0), IFont.Pixel16, TextLine.OffsetType.MIDDLE);
			warning.WriteText(new Color(0.5f, 0.5f, 1f) + "WS is still under development. If you encounter any bugs, please report them. Thanks for playing!");

			move = new TextLine(new CPos(0, 3072, 0), IFont.Pixel16, TextLine.OffsetType.MIDDLE);
			move.WriteText("Move: " + Color.Yellow + "W, A, S, D");

			attack = new TextLine(new CPos(0, 2048, 0), IFont.Pixel16, TextLine.OffsetType.MIDDLE);
			attack.WriteText("Attack: " + Color.Yellow + "Left mouse button");

			aim = new TextLine(CPos.Zero, IFont.Pixel16, TextLine.OffsetType.MIDDLE);
			aim.SetText("Go up, and you'll get to the story mode. Go down for the tutorial.");

			how = new TextLine(new CPos(0, 1024, 0), IFont.Pixel16, TextLine.OffsetType.MIDDLE);
			how.SetText("");

			@switch = new TextLine(new CPos(0, 4096, 0), IFont.Pixel16, TextLine.OffsetType.MIDDLE);
			@switch.WriteText("Switch through characters: " + Color.Cyan + "Mouse scroll" + Color.White + " (Requires Money)");
		}

		public override void Tick()
		{
			base.Tick();

			back.Tick();
			exit.Tick();
			warning.Tick();
			move.Tick();
			aim.Tick();
			how.Tick();
			attack.Tick();
			@switch.Tick();
		}

		public override void Render()
		{
			base.Render();

			back.Render();
			exit.Render();
			warning.Render();
			move.Render();
			aim.Render();
			how.Render();
			attack.Render();
			@switch.Render();
		}

		public override void Dispose()
		{
			base.Dispose();

			back.Dispose();
			exit.Dispose();
			warning.Dispose();
			move.Dispose();
			aim.Dispose();
			how.Dispose();
			attack.Dispose();
			@switch.Dispose();
		}
	}
}
