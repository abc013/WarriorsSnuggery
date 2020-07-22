using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI
{
	public class MoneyDisplay : UIObject
	{
		public override CPos Position
		{
			get => base.Position;
			set
			{
				base.Position = value;
				money.SetPosition(value);
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
		readonly TextLine moneyText;
		int cashCooldown;
		int lastCash;

		public MoneyDisplay(Game game, CPos position)
		{
			this.game = game;
			money = new BatchObject(UITextureManager.Get("UI_money")[0], Color.White);
			money.SetPosition(position);

			moneyText = new TextLine(position + new CPos(1024, 0, 0), FontManager.Papyrus24);
			moneyText.SetText(game.Statistics.Money);
		}

		public override void Tick()
		{
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
			money.PushToBatchRenderer();
			moneyText.Render();
		}
	}
}
