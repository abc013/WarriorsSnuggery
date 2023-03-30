using System.Collections.Generic;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.UI.Objects
{
	public sealed class HealthDisplay : Panel, ITick
	{
		public override UIPos Position
		{
			get => base.Position;
			set
			{
				base.Position = value;

				for (var i = 0; i < hearts.Count; i++)
					setPosition(i);
			}
		}

		readonly Game game;
		readonly BatchObject bigHeart;
		readonly List<BatchObject> hearts = new List<BatchObject>();
		int lifes;
		const int maxTick = 120;
		int tick;

		public HealthDisplay(Game game) : base(new UIPos(712 + 128, 512), PanelCache.Types["wooden"])
		{
			this.game = game;

			lifes = game.Player.Lifes;

			bigHeart = new BatchObject(UISpriteManager.Get("UI_heart")[0]);
		}

		public void Tick()
		{
			while (hearts.Count < game.Player.MaxLifes)
			{
				var heart = new BatchObject(UISpriteManager.Get("UI_heart")[0]);
				hearts.Add(heart);

				for (var i = 0; i < hearts.Count; i++)
					setPosition(i);
			}

			var currentLifes = game.Player.Lifes;
			if (lifes > currentLifes)
			{
				UIUtils.PlayLifeLostSound();
				tick = maxTick;
			}

			lifes = currentLifes;
		}

		void setPosition(int i)
		{
			// 3 per row
			var x = -512 + 512 * (i % 3);
			var y = hearts.Count > 3 ? -256 + 512 * (i / 3) : 0;

			hearts[i].SetPosition(Position + new UIPos(x, y));
		}

		public override void Render()
		{
			base.Render();

			for (var i = 0; i < hearts.Count; i++)
			{
				var heart = hearts[i];

				heart.SetColor(i < game.Player.Lifes ? Color.White : Color.Black);
				heart.Render();

				if (i == lifes && tick-- > 0)
				{
					heart.SetColor(new Color(1f, 1f, 1f, tick / (float)maxTick));
					heart.SetScale((maxTick - tick)/(float)maxTick * 10);
					heart.Render();

					heart.SetScale(1f);
				}
			}

			if (tick-- > 0)
			{
				bigHeart.SetColor(new Color(1f, 1f, 1f, tick / (float)maxTick));
				bigHeart.SetScale((maxTick - tick)/(float)maxTick * 100);
				bigHeart.Render();
			}
		}
	}
}
