using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.UI
{
	public class MoneyDisplay : Panel
	{
		public override CPos Position
		{
			get => base.Position;
			set
			{
				base.Position = value;

				if (money != null)
				{
					money.SetPosition(value);
					moneyText.Position = value;
				}
			}
		}

		public override VAngle Rotation
		{
			get => base.Rotation;
			set
			{
				base.Rotation = value;

				if (money != null)
				{
					money.SetRotation(value);
					moneyText.Rotation = value;
				}
			}
		}

		public override float Scale
		{
			get => base.Scale;
			set
			{
				base.Scale = value;

				if (money != null)
				{
					money.SetScale(value);
					moneyText.Scale = value;
				}
			}
		}

		readonly Game game;
		readonly BatchObject money;
		readonly UITextLine moneyText;
		int cashCooldown;
		int lastCash;

		public MoneyDisplay(Game game, CPos position) : base(position, new MPos(1536, 512), PanelManager.Get("wooden"))
		{
			this.game = game;
			money = new BatchObject(UITextureManager.Get("UI_money")[0], Color.White);
			money.SetPosition(position - new CPos(1024, 0, 0));

			moneyText = new UITextLine(position, FontManager.Pixel16);
			moneyText.SetText(game.Statistics.Money);
		}

		public override void Tick()
		{
			base.Tick();

			if (lastCash != game.Statistics.Money)
			{
				lastCash = game.Statistics.Money;
				moneyText.SetText(game.Statistics.Money);
				cashCooldown = 10;
			}

			if (cashCooldown-- > 0)
				moneyText.Scale = (cashCooldown / 10f) + 1f;
		}

		public override void Render()
		{
			base.Render();

			money.PushToBatchRenderer();
			moneyText.Render();
		}
	}
}
