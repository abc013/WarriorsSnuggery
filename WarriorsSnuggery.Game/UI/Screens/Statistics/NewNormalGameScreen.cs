using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI.Screens
{
	public class NewNormalGameScreen : Screen
	{
		readonly Game game;

		readonly TextBox nameInput;
		readonly SliderBar difficultyInput;
		readonly CheckBox hardcoreInput;
		readonly TextBox seedInput;

		public NewNormalGameScreen(Game game) : base("Free Play")
		{
			this.game = game;
			Title.Position = new CPos(0, -4096, 0);

			var create = new UITextLine(new CPos(0, -2048, 0), FontManager.Pixel16, TextOffset.MIDDLE);
			create.SetText("Free play. Please adjust the parameters as you wish.");
			Content.Add(create);

			var name = new UITextLine(new CPos(-2048, 0, 0), FontManager.Pixel16, TextOffset.RIGHT);
			name.SetText("Name: ");
			Content.Add(name);

			nameInput = new TextBox(new CPos(1024, 0, 0), "Name", "wooden", 15, isPath: true);
			Content.Add(nameInput);

			var difficulty = new UITextLine(new CPos(-2048, 1024, 0), FontManager.Pixel16, TextOffset.RIGHT);
			difficulty.SetText("Difficulty: ");
			Content.Add(difficulty);

			difficultyInput = new SliderBar(new CPos(1024, 1024, 0), 4096, "wooden");
			Content.Add(difficultyInput);

			var hardcore = new UITextLine(new CPos(-2048, 2048, 0), FontManager.Pixel16, TextOffset.RIGHT);
			hardcore.SetText("Hardcore (one life): ");
			Content.Add(hardcore);

			hardcoreInput = new CheckBox("wooden")
			{
				Position = new CPos(1024, 2048, 0)
			};
			Content.Add(hardcoreInput);

			var seed = new UITextLine(new CPos(-2048, 3072, 0), FontManager.Pixel16, TextOffset.RIGHT);
			seed.SetText("Seed: ");
			Content.Add(seed);

			seedInput = new TextBox(new CPos(1024, 3072, 0), getSeed(), "wooden", 7, true);
			Content.Add(seedInput);
			Content.Add(new Button(new CPos(6144, 3072, 0), "Generate", "wooden", () => { seedInput.Text = getSeed(); }));

			Content.Add(new Button(new CPos(-4096, 6144, 0), "Cancel", "wooden", () => game.ShowScreen(ScreenType.DEFAULT, false)));
			Content.Add(new Button(new CPos(4096, 6144, 0), "Proceed", "wooden", () =>
			{
				if (nameInput.Text != string.Empty)
					GameController.CreateNew(new GameStatistics((int)Math.Round(difficultyInput.Value * 10), hardcoreInput.Checked, nameInput.Text, int.Parse(seedInput.Text)));
			}));
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
			if (key == Keys.Escape)
				game.ShowScreen(ScreenType.DEFAULT, false);
		}
	}
}
