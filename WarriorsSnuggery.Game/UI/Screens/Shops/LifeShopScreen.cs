using OpenTK.Windowing.GraphicsLibraryFramework;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.UI.Objects;

namespace WarriorsSnuggery.UI.Screens
{
	public class LifeShopScreen : Screen
	{
		readonly Game game;
		readonly UIText information, price;

		public LifeShopScreen(Game game) : base("Lifes")
		{
			this.game = game;
			Title.Position = new UIPos(0, -4096);

			Add(new MoneyDisplay(game) { Position = new UIPos(Left + 2048, Bottom - 1024) });
			Add(new HealthDisplay(game) { Position = new UIPos(0, -2048) });

			information = new UIText(FontManager.Default, TextOffset.MIDDLE) { Position = new UIPos(0, -2048 - 712) };
			Add(information);

			price = new UIText(FontManager.Default, TextOffset.MIDDLE) { Position = UIPos.Zero };
			Add(price);

			Add(new Button("Buy life", "wooden", buyLife) { Position = new UIPos(0, 1024) });
			Add(new Button("Resume", "wooden", () => game.ShowScreen(ScreenType.DEFAULT, false)) { Position = new UIPos(0, 6144) });

			updateInformation();
		}

		public override void Show()
		{
			updateInformation();
		}

		void buyLife()
		{
			var stats = game.Player;

			var nextPrice = stats.NextLifePrice;

			if (nextPrice > stats.Money)
				return;

			UIUtils.PlaySellSound();

			stats.Money -= nextPrice;
			stats.Lifes++;

			updateInformation();
		}

		void updateInformation()
		{
			var stats = game.Player;

			if (stats.Lifes == stats.MaxLifes)
			{
				information.Color = Color.Green;
				information.SetText("Max life limit reached!");
			}
			else
			{
				information.Color = Color.White;
				information.SetText($"Current: {stats.Lifes}/{stats.MaxLifes}");
			}

			var nextPrice = stats.NextLifePrice;
			price.Color = nextPrice > stats.Money ? Color.Red : Color.Green;
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
