using System.Collections.Generic;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.UI.Objects
{
	public class HealthDisplay : Panel
	{
		public override CPos Position
		{
			get => base.Position;
			set
			{
				base.Position = value;

				for (var i = 0; i < hearts.Count; i++)
					setPosition(i);
			}
		}

		public override VAngle Rotation
		{
			get => base.Rotation;
			set
			{
				base.Rotation = value;

				for (var i = 0; i < hearts.Count; i++)
					hearts[i].SetRotation(value);
			}
		}

		public override float Scale
		{
			get => base.Scale;
			set
			{
				base.Scale = value;

				for (var i = 0; i < hearts.Count; i++)
					hearts[i].SetScale(value);
			}
		}

		readonly Game game;
		readonly BatchObject bigHeart;
		readonly List<BatchObject> hearts = new List<BatchObject>();
		int lifes;
		const int maxTick = 120;
		int tick;

		public HealthDisplay(Game game) : base(new MPos(712 + 128, 512), PanelCache.Types["wooden"])
		{
			this.game = game;

			lifes = game.Stats.Lifes;

			bigHeart = new BatchObject(UISpriteManager.Get("UI_heart")[0]);
		}

		public override void Tick()
		{
			base.Tick();

			while (hearts.Count < game.Stats.MaxLifes)
			{
				var heart = new BatchObject(UISpriteManager.Get("UI_heart")[0]);
				hearts.Add(heart);

				for (var i = 0; i < hearts.Count; i++)
					setPosition(i);
			}

			var currentLifes = game.Stats.Lifes;
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

			hearts[i].SetPosition(Position + new CPos(x, y, 0));
		}

		public override void Render()
		{
			base.Render();

			for (var i = 0; i < hearts.Count; i++)
			{
				var heart = hearts[i];

				heart.SetColor(i < game.Stats.Lifes ? Color.White : Color.Black);
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
