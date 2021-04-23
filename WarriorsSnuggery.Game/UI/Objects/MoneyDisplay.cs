using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.UI.Objects
{
	public class MoneyDisplay : Panel
	{
		public override CPos Position
		{
			get => base.Position;
			set
			{
				base.Position = value;

				money.SetPosition(value - new CPos(1024, 0, 0));
				moneyText.Position = value;
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
		readonly UITextLine moneyText;
		int cashCooldown;
		int lastCash;

		public MoneyDisplay(Game game) : base(new MPos(1536, 512), PanelManager.Get("wooden"))
		{
			this.game = game;
			money = new BatchObject(UITextureManager.Get("UI_money")[0]);

			moneyText = new UITextLine(FontManager.Pixel16);
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
