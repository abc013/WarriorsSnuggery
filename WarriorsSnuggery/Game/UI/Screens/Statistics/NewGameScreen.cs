using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI
{
	public class NewGameScreen : Screen
	{
		readonly Game game;

		readonly TextLine create, name, difficulty, hardcore, seed;
		readonly TextBox nameInput;
		readonly TextBox difficultyInput;
		readonly CheckBox hardcoreInput;
		readonly TextBox seedInput;
		readonly Button seedGenerate;

		readonly Button cancel;
		readonly Button proceed;

		public NewGameScreen(Game game) : base("New Game [STORYMODE]")
		{
			this.game = game;
			Title.Position = new CPos(0, -4096, 0);

			create = new TextLine(new CPos(-2048, -2048, 0), IFont.Pixel16, TextLine.OffsetType.MIDDLE);
			create.SetText("Please adjust the parameters as you wish.");

			name = new TextLine(new CPos(-2048, 0, 0), IFont.Pixel16, TextLine.OffsetType.RIGHT);
			name.SetText("Name: ");

			nameInput = TextBoxCreator.Create("wooden", new CPos(1024, 0, 0), "Name", 15);

			difficulty = new TextLine(new CPos(-2048, 1024, 0), IFont.Pixel16, TextLine.OffsetType.RIGHT);
			difficulty.SetText("Difficulty: ");

			difficultyInput = TextBoxCreator.Create("wooden", new CPos(1024, 1024, 0), "1", 1, true);

			hardcore = new TextLine(new CPos(-2048, 2048, 0), IFont.Pixel16, TextLine.OffsetType.RIGHT);
			hardcore.SetText("Hardcore: ");

			hardcoreInput = CheckBoxCreator.Create("wooden", new CPos(1024, 2048, 0), false);

			seed = new TextLine(new CPos(-2048, 3072, 0), IFont.Pixel16, TextLine.OffsetType.RIGHT);
			seed.SetText("Seed: ");

			seedInput = TextBoxCreator.Create("wooden", new CPos(1024, 3072, 0), getSeed(), 7, true);
			seedGenerate = ButtonCreator.Create("wooden", new CPos(6144, 3072, 0), "Generate", () => { seedInput.Text = getSeed(); });

			cancel = ButtonCreator.Create("wooden", new CPos(-4096, 6144, 0), "Cancel", () => { game.Pause(false); game.ChangeScreen(ScreenType.DEFAULT); });
			proceed = ButtonCreator.Create("wooden", new CPos(4096, 6144, 0), "Proceed", () => { GameController.CreateNew(GameStatistics.CreateGameStatistic(int.Parse(difficultyInput.Text), hardcoreInput.Checked, nameInput.Text, int.Parse(seedInput.Text))); });
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

			create.Tick();
			name.Tick();
			difficulty.Tick();
			hardcore.Tick();
			seed.Tick();

			nameInput.Tick();
			difficultyInput.Tick();
			hardcoreInput.Tick();
			seedInput.Tick();
			seedGenerate.Tick();

			cancel.Tick();
			proceed.Tick();

			if (KeyInput.IsKeyDown("escape", 10))
			{
				game.Pause(false);
				game.ChangeScreen(ScreenType.DEFAULT);
			}
		}

		public override void Render()
		{
			base.Render();

			create.Render();
			name.Render();
			difficulty.Render();
			hardcore.Render();
			seed.Render();

			nameInput.Render();
			difficultyInput.Render();
			hardcoreInput.Render();
			seedInput.Render();
			seedGenerate.Render();

			cancel.Render();
			proceed.Render();
		}

		public override void Dispose()
		{
			base.Dispose();

			create.Dispose();
			name.Dispose();
			difficulty.Dispose();
			hardcore.Dispose();
			seed.Dispose();

			nameInput.Dispose();
			difficultyInput.Dispose();
			hardcoreInput.Dispose();
			seedInput.Dispose();
			seedGenerate.Dispose();

			cancel.Dispose();
		}
	}
}
