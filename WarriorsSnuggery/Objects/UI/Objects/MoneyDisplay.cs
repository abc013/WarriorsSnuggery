using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI
{
	public class MoneyDisplay : IPositionable, ITickRenderable
	{
		public virtual CPos Position
		{
			get { return position; }
			set
			{
				position = value;
				money.SetPosition(position);
				moneyText.Position = position;
			}
		}
		CPos position;

		public virtual VAngle Rotation
		{
			get { return rotation; }
			set
			{
				rotation = value;
				money.SetRotation(rotation);
				moneyText.Rotation = rotation;
			}
		}
		VAngle rotation;

		public virtual float Scale
		{
			get { return scale; }
			set
			{
				scale = value;
				money.SetScale(scale);
				moneyText.Scale = scale;
			}
		}
		float scale = 1f;

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

		public void Tick()
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

		public void Render()
		{
			money.PushToBatchRenderer();
			moneyText.Render();
		}
	}
}
