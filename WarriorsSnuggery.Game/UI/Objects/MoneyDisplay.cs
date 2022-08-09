using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.UI.Objects
{
	public class MoneyDisplay : Panel
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

		public override VAngle Rotation
		{
			get => base.Rotation;
			set
			{
				base.Rotation = value;

				money.SetRotation(value);
				moneyText.Rotation = value;
			}
		}

		public override float Scale
		{
			get => base.Scale;
			set
			{
				base.Scale = value;

				money.SetScale(value);
				moneyText.Scale = value;
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
			moneyText.SetText(game.Stats.Money);
		}

		public override void Tick()
		{
			base.Tick();

			if (lastCash != game.Stats.Money)
			{
				lastCash = game.Stats.Money;
				moneyText.SetText(game.Stats.Money);
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
