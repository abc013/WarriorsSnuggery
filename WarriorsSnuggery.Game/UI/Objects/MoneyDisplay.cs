using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.UI.Objects
{
	public sealed class MoneyDisplay : Panel, ITick
	{
		public override UIPos Position
		{
			get => base.Position;
			set
			{
				base.Position = value;

				money.SetPosition(value - new UIPos(1024, 0));
				moneyText.Position = value + new UIPos(512, 64);
			}
		}

		readonly Game game;
		readonly BatchObject money;
		readonly UIText moneyText;
		int cashCooldown;
		int lastCash;

		public MoneyDisplay(Game game) : base(new UIPos(1536, 512), PanelCache.Types["wooden"])
		{
			this.game = game;
			money = new BatchObject(UISpriteManager.Get("UI_money")[0]);

			moneyText = new UIText(FontManager.Header, TextOffset.MIDDLE);
			moneyText.SetText(game.Player.Money);
		}

		public void Tick()
		{
			if (lastCash != game.Player.Money)
			{
				lastCash = game.Player.Money;
				moneyText.SetText(game.Player.Money);
				cashCooldown = 10;
			}

			if (cashCooldown-- > 0)
				moneyText.Scale = (cashCooldown / 10f) + 1f;
		}

		public override void Render()
		{
			base.Render();

			money.Render();
			moneyText.Render();
		}
	}
}
