using System;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI
{
	public class NewGameScreen : Screen
	{
		readonly Game game;

		readonly Text create, name, difficulty, hardcore;
		readonly TextBox nameInput;
		readonly TextBox difficultyInput;
		readonly CheckBox hardcoreInput;

		readonly Button cancel;
		readonly Button proceed;

		public NewGameScreen(Game game) : base("New Game [STORYMODE]")
		{
			this.game = game;
			Title.Position = new CPos(0, -4096, 0);

			create = new Text(new CPos(-2048, -2048, 0), IFont.Pixel16, Text.OffsetType.MIDDLE);
			create.SetText("Please adjust the parameters as you wish.");

			name = new Text(new CPos(-2048, 0, 0), IFont.Pixel16, Text.OffsetType.RIGHT);
			name.SetText("Name: ");

			nameInput = TextBoxCreator.Create("wooden", new CPos(1024, 0, 0), "Name", 15);

			difficulty = new Text(new CPos(-2048, 1024, 0), IFont.Pixel16, Text.OffsetType.RIGHT);
			difficulty.SetText("Difficulty: ");

			difficultyInput = TextBoxCreator.Create("wooden", new CPos(1024, 1024, 0), "1", 1, true);

			hardcore = new Text(new CPos(-2048, 2048, 0), IFont.Pixel16, Text.OffsetType.RIGHT);
			hardcore.SetText("Hardcore: ");

			hardcoreInput = CheckBoxCreator.Create("wooden", new CPos(1024, 2048, 0), false);

			cancel = ButtonCreator.Create("wooden", new CPos(-4096, 6144, 0), "Cancel", () => { game.ChangeScreen(ScreenType.DEFAULT); game.Pause(false); });
			proceed = ButtonCreator.Create("wooden", new CPos(4096, 6144, 0), "Proceed", () => { Window.Current.NewGame(GameStatistics.CreateGameStatistic(int.Parse(difficultyInput.Text), hardcoreInput.Checked, nameInput.Text)); });
		}

		public override void Tick()
		{
			base.Tick();

			create.Tick();
			name.Tick();
			difficulty.Tick();
			hardcore.Tick();

			nameInput.Tick();
			difficultyInput.Tick();
			hardcoreInput.Tick();

			cancel.Tick();
			proceed.Tick();

			if (KeyInput.IsKeyDown("escape", 10))
			{
				game.ChangeScreen(ScreenType.DEFAULT);
				game.Pause(false);
			}
		}

		public override void Render()
		{
			base.Render();

			create.Render();
			name.Render();
			difficulty.Render();
			hardcore.Render();

			nameInput.Render();
			difficultyInput.Render();
			hardcoreInput.Render();

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

			nameInput.Dispose();
			difficultyInput.Dispose();
			hardcoreInput.Dispose();

			cancel.Dispose();
		}
	}
}
