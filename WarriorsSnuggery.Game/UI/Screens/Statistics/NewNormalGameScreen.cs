using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;
using WarriorsSnuggery.UI.Objects;

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

			var create = new UITextLine(FontManager.Pixel16, TextOffset.MIDDLE) { Position = new CPos(0, -2048, 0) };
			create.SetText("Free play. Please adjust the parameters as you wish.");
			Add(create);

			var name = new UITextLine(FontManager.Pixel16, TextOffset.RIGHT) { Position = new CPos(-2048, 0, 0) };
			name.SetText("Name: ");
			Add(name);

			nameInput = new TextBox("Name", "wooden", 15, isPath: true) { Position = new CPos(1024, 0, 0) };
			Add(nameInput);

			var difficulty = new UITextLine(FontManager.Pixel16, TextOffset.RIGHT) { Position = new CPos(-2048, 1024, 0) };
			difficulty.SetText("Difficulty: ");
			Add(difficulty);

			difficultyInput = new SliderBar(4096, "wooden") { Position = new CPos(1024, 1024, 0) };
			Add(difficultyInput);

			var hardcore = new UITextLine(FontManager.Pixel16, TextOffset.RIGHT) { Position = new CPos(-2048, 2048, 0) };
			hardcore.SetText("Hardcore (one life): ");
			Add(hardcore);

			hardcoreInput = new CheckBox("wooden") { Position = new CPos(1024, 2048, 0) };
			Add(hardcoreInput);

			var seed = new UITextLine(FontManager.Pixel16, TextOffset.RIGHT) { Position = new CPos(-2048, 3072, 0) };
			seed.SetText("Seed: ");
			Add(seed);

			seedInput = new TextBox(getSeed(), "wooden", 7, true) { Position = new CPos(1024, 3072, 0) };
			Add(seedInput);
			Add(new Button("Generate", "wooden", () => { seedInput.Text = getSeed(); }) { Position = new CPos(6144, 3072, 0) });

			Add(new Button("Cancel", "wooden", () => game.ShowScreen(ScreenType.DEFAULT, false)) { Position = new CPos(-4096, 6144, 0) });
			Add(new Button("Proceed", "wooden", () =>
			{
				if (!string.IsNullOrWhiteSpace(nameInput.Text))
					GameController.CreateNew(new GameSave((int)Math.Round(difficultyInput.Value * 10), hardcoreInput.Checked, nameInput.Text, int.Parse(seedInput.Text)));
			})
			{ Position = new CPos(4096, 6144, 0) });
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
