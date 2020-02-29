using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI
{
	public class NewGameScreen : Screen
	{
		readonly Game game;

		readonly TextBox nameInput;
		readonly TextBox difficultyInput;
		readonly CheckBox hardcoreInput;
		readonly TextBox seedInput;

		public NewGameScreen(Game game) : base("New Game [STORYMODE]")
		{
			this.game = game;
			Title.Position = new CPos(0, -4096, 0);

			var create = new TextLine(new CPos(0, -2048, 0), Font.Pixel16, TextLine.OffsetType.MIDDLE);
			create.SetText("Please adjust the parameters as you wish.");
			Content.Add(create);

			var name = new TextLine(new CPos(-2048, 0, 0), Font.Pixel16, TextLine.OffsetType.RIGHT);
			name.SetText("Name: ");
			Content.Add(name);

			nameInput = TextBoxCreator.Create("wooden", new CPos(1024, 0, 0), "Name", 15, isPath: true);

			var difficulty = new TextLine(new CPos(-2048, 1024, 0), Font.Pixel16, TextLine.OffsetType.RIGHT);
			difficulty.SetText("Difficulty: ");
			Content.Add(difficulty);

			difficultyInput = TextBoxCreator.Create("wooden", new CPos(1024, 1024, 0), "1", 1, true);

			var hardcore = new TextLine(new CPos(-2048, 2048, 0), Font.Pixel16, TextLine.OffsetType.RIGHT);
			hardcore.SetText("Hardcore: ");
			Content.Add(hardcore);

			hardcoreInput = CheckBoxCreator.Create("wooden", new CPos(1024, 2048, 0), false);

			var seed = new TextLine(new CPos(-2048, 3072, 0), Font.Pixel16, TextLine.OffsetType.RIGHT);
			seed.SetText("Seed: ");
			Content.Add(seed);

			seedInput = TextBoxCreator.Create("wooden", new CPos(1024, 3072, 0), getSeed(), 7, true);
			Content.Add(ButtonCreator.Create("wooden", new CPos(6144, 3072, 0), "Generate", () => { seedInput.Text = getSeed(); }));

			Content.Add(ButtonCreator.Create("wooden", new CPos(-4096, 6144, 0), "Cancel", () => { game.Pause(false); game.ChangeScreen(ScreenType.DEFAULT); }));
			Content.Add(ButtonCreator.Create("wooden", new CPos(4096, 6144, 0), "Proceed", () =>
			{
				if (nameInput.Text != string.Empty)
					GameController.CreateNew(GameStatistics.CreateGameStatistic(int.Parse(difficultyInput.Text), hardcoreInput.Checked, nameInput.Text, int.Parse(seedInput.Text)));
			}));
		}

		string getSeed()
		{
			var ran = game.SharedRandom.Next() + "";
			if (ran.Length > 8)
				ran = ran.Remove(8);

			return ran;
		}

		public override void Tick()
		{
			base.Tick();

			nameInput.Tick();
			difficultyInput.Tick();
			hardcoreInput.Tick();
			seedInput.Tick();

			if (KeyInput.IsKeyDown("escape", 10))
			{
				game.Pause(false);
				game.ChangeScreen(ScreenType.DEFAULT);
			}
		}

		public override void Render()
		{
			base.Render();

			nameInput.Render();
			difficultyInput.Render();
			hardcoreInput.Render();
			seedInput.Render();
		}
	}
}
