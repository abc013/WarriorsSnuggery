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
					hearts[i].SetPosition(value + new CPos(-712 + 512 * hearts.Count, 0, 0));
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
		readonly List<BatchObject> hearts = new List<BatchObject>();
		int lifes;
		int tick;

		public HealthDisplay(Game game) : base(new MPos(712 + 128, 512), PanelManager.Get("wooden"))
		{
			this.game = game;

			lifes = game.Stats.Lifes;
		}

		public override void Tick()
		{
			base.Tick();

			while (hearts.Count < game.Stats.MaxLifes)
			{
				var heart = new BatchObject(UISpriteManager.Get("UI_heart")[0]);
				heart.SetPosition(Position + new CPos(-512 + 512 * hearts.Count, 0, 0));
				hearts.Add(heart);
			}

			var currentLifes = game.Stats.Lifes;
			if (lifes > currentLifes)
				tick = 120;

			lifes = currentLifes;
		}

		public override void Render()
		{
			base.Render();

			for (var i = 0; i < hearts.Count; i++)
			{
				var heart = hearts[i];

				heart.SetColor(i < game.Stats.Lifes ? Color.White : Color.Black);
				heart.PushToBatchRenderer();

				if (i == lifes && tick-- > 0)
				{
					heart.SetColor(new Color(1f, 1f, 1f, tick / 140f));
					heart.SetScale((140 - tick)/14f);
					heart.PushToBatchRenderer();
					heart.SetScale(1f);
				}
			}
		}
	}
}
