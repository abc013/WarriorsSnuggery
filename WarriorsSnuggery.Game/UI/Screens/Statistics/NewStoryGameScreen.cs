using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.UI.Objects;

namespace WarriorsSnuggery.UI.Screens
{
	public class NewStoryGameScreen : Screen
	{
		readonly Game game;

		readonly SliderBar difficultyInput;
		readonly CheckBox hardcoreInput;
		readonly TextBox seedInput;

		public NewStoryGameScreen(Game game) : base("Play Story")
		{
			this.game = game;
			Title.Position = new UIPos(0, -4096);

			var create = new UIText(FontManager.Default, TextOffset.MIDDLE) { Position = new UIPos(0, -2048) };
			create.SetText("Story line. Please adjust the parameters as you wish.", $"{Color.Cyan}Note: The Story line is not implemented yet.");
			Add(create);

			var difficulty = new UIText(FontManager.Default, TextOffset.RIGHT) { Position = new UIPos(-2048, 1024) };
			difficulty.SetText("Difficulty: ");
			Add(difficulty);

			difficultyInput = new SliderBar(4096, "wooden", tooltipDigits: 0, valueMultiplier: 10) { Position = new UIPos(1024, 1024) };
			Add(difficultyInput);

			var hardcore = new UIText(FontManager.Default, TextOffset.RIGHT) { Position = new UIPos(0, 2048) };
			hardcore.SetText("Hardcore (one life): ");
			Add(hardcore);

			hardcoreInput = new CheckBox("wooden")
			{
				Position = new UIPos(1024, 2048)
			};
			Add(hardcoreInput);

			var seed = new UIText(FontManager.Default, TextOffset.RIGHT) { Position = new UIPos(-2048, 3072) };
			seed.SetText("Seed: ");
			Add(seed);

			seedInput = new TextBox("wooden", 7, InputType.NUMBERS)
			{
				Position = new UIPos(1024, 3072),
				Text = getSeed()
			};
			Add(seedInput);
			Add(new Button("Generate", "wooden", () => { seedInput.Text = getSeed(); }) { Position = new UIPos(6144, 3072) });

			Add(new Button("Cancel", "wooden", () => game.ShowScreen(ScreenType.DEFAULT, false)) { Position = new UIPos(-4096, 6144) });
			Add(new Button("Proceed", "wooden", () =>
			{
				if (!string.IsNullOrWhiteSpace(seedInput.Text))
					GameController.CreateNew(new GameSave((int)Math.Round(difficultyInput.Value), hardcoreInput.Checked, "New Game", int.Parse(seedInput.Text)), MissionType.STORY);
			}) { Position = new UIPos(4096, 6144) });
		}

		string getSeed()
		{
			var ran = game.SharedRandom.Next() + "";
			if (ran.Length > 8)
				ran = ran.Remove(8);

			return ran;
		}

		public override void KeyDown(Keys key, bool isControl, bool isShift, bool isAlt)
		{
			base.KeyDown(key, isControl, isShift, isAlt);

			if (key == Keys.Escape)
				game.ShowScreen(ScreenType.DEFAULT, false);
		}
	}
}
