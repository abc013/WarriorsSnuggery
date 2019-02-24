/*
 * User: Andreas
 * Date: 13.10.2018
 * Time: 19:34
 */
using System;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI
{
	public class StartScreen : Screen
	{
		readonly Button back;
		readonly Button exit;

		readonly Text warning;
		readonly Text move;
		readonly Text attack;
		readonly Text @switch;
		readonly Text aim;
		readonly Text how;

		readonly Game game;

		public StartScreen(Game game) : base("Welcome!")
		{
			this.game = game;
			Title.Position = new CPos(0,-4096, 0);

			back = ButtonCreator.Create("wooden", new CPos(4096, 6144, 0), "Okay", game.Pause);
			exit = ButtonCreator.Create("wooden", new CPos(-4096, 6144, 0), "Exit", Window.Current.Exit);

			warning = new Text(new CPos(0, 5500, 0), IFont.Pixel16, Text.OffsetType.MIDDLE);
			warning.WriteText(new Color(0.5f, 0.5f, 1f) + "WS is still under development. If you encounter any bugs, please report them. Thanks for playing!");

			move = new Text(new CPos(0, 3072, 0), IFont.Pixel16, Text.OffsetType.MIDDLE);
			move.WriteText("Move: " + Color.Yellow + "W, A, S, D");

			attack = new Text(new CPos(0, 2048, 0), IFont.Pixel16, Text.OffsetType.MIDDLE);
			attack.WriteText("Attack: " + Color.Yellow + "Left mouse button");

			aim = new Text(CPos.Zero, IFont.Pixel16, Text.OffsetType.MIDDLE);
			aim.SetText("Goal is to reach the " + game.Statistics.LevelToReach + "th Level without dying. If you happen to achieve that, you'll get an ice cream.");

			how = new Text(new CPos(0, 1024, 0), IFont.Pixel16, Text.OffsetType.MIDDLE);
			how.SetText("You will start with 4 friendly towers. To reach the next level, find the exit!");

			@switch = new Text(new CPos(0, 4096, 0), IFont.Pixel16, Text.OffsetType.MIDDLE);
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
