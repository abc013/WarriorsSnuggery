using System;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI
{
	public class TechTreeScreen : Screen
	{
		readonly Game game;

		readonly Button back;

		readonly Panel @base;

		public TechTreeScreen(Game game) : base("Tech Tree")
		{
			this.game = game;
			Title.Position = new CPos(0, -4096, 0);

			back = ButtonCreator.Create("wooden", new CPos(0, 6144, 0), "Resume", () => Window.Current.NewGame(game.Statistics, sameSeed: true));
			@base = new Panel(new CPos(0, 1024, 0), new MPos(8192 / 64 * 3, 4048 / 64 * 3), 4, "UI_wood1", "UI_wood3", "UI_wood2");
		}

		public override void Render()
		{
			base.Render();
			@base.Render();

			back.Render();
		}

		public override void Tick()
		{
			base.Tick();
			@base.Tick();

			back.Tick();
		}

		public override void Dispose()
		{
			base.Dispose();
			@base.Dispose();

			back.Dispose();
		}
	}

	class TechNode : Panel
	{
		public TechNode(CPos position) : base(position, new MPos(2048, 512), 2, "UI_stone1", "UI_stone2", "")
		{

		}
	}
}
