using OpenTK.Windowing.GraphicsLibraryFramework;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;
using WarriorsSnuggery.UI.Objects;

namespace WarriorsSnuggery.UI.Screens
{
	public class LifeShopScreen : Screen
	{
		readonly Game game;
		readonly UITextLine information, price;

		public LifeShopScreen(Game game) : base("Lifes")
		{
			this.game = game;
			Title.Position = new CPos(0, -4096, 0);

			Add(new MoneyDisplay(game) { Position = new CPos(Left + 2048, Bottom - 1024, 0) });
			Add(new HealthDisplay(game) { Position = new CPos(0, -2048, 0) });

			information = new UITextLine(FontManager.Default, TextOffset.MIDDLE) { Position = new CPos(0, -2048 - 712, 0) };
			Add(information);

			price = new UITextLine(FontManager.Default, TextOffset.MIDDLE) { Position = new CPos(0, 0, 0) };
			Add(price);

			Add(new Button("Buy life", "wooden", buyLife) { Position = new CPos(0, 1024, 0) });
			Add(new Button("Resume", "wooden", () => game.ShowScreen(ScreenType.DEFAULT, false)) { Position = new CPos(0, 6144, 0) });

			updateInformation();
		}

		public override void Show()
		{
			updateInformation();
		}

		void buyLife()
		{
			var stats = game.Stats;

			var nextPrice = stats.NextLifePrice();

			if (nextPrice > stats.Money)
				return;

			UIUtils.PlaySellSound();

			stats.Money -= nextPrice;
			stats.Lifes++;

			updateInformation();
		}

		void updateInformation()
		{
			var stats = game.Stats;

			if (stats.Lifes == stats.MaxLifes)
			{
				information.SetColor(Color.Green);
				information.SetText("Max life limit reached!");
			}
			else
			{
				information.SetColor(Color.White);
				information.SetText($"Current: {stats.Lifes}/{stats.MaxLifes}");
			}

			var nextPrice = stats.NextLifePrice();
			price.SetColor(nextPrice > stats.Money ? Color.Red : Color.Green);
			price.SetText($"Current Price: {nextPrice}");
		}

		public override void KeyDown(Keys key, bool isControl, bool isShift, bool isAlt)
		{
			base.KeyDown(key, isControl, isShift, isAlt);

			if (key == Keys.Escape)
				game.ShowScreen(ScreenType.DEFAULT, false);
		}
	}
}
