using System;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.UI
{
	class GameSaveItem : PanelItem
	{
		public override CPos Position
		{
			get => base.Position;
			set
			{
				base.Position = value;
				name.Position = value - new CPos(3072, 512, 0);
				score.Position = value - new CPos(3072, 0, 0);
				level.Position = value - new CPos(-1152, 0, 0);
				tooltip.Position = value;
			}
		}

		public readonly GameStatistics Stats;
		public bool Selected;

		readonly UITextLine name;
		readonly UITextLine score;
		readonly UITextLine level;

		public GameSaveItem(GameStatistics stats, int width, Action action) : base(new BatchObject(UITextureManager.Get("UI_save")[0], Color.White), new MPos(width, 1024), stats.Name, new[] { Color.Grey + "Difficulty: " + stats.Difficulty, Color.Grey + "Money: " + stats.Money }, action)
		{
			var pos = CPos.Zero;

			Stats = stats;
			Scale *= 2;
			
			name = new UITextLine(pos - new CPos(3072, 512, 0), FontManager.Pixel16);
			name.SetText(stats.Name);

			score = new UITextLine(pos - new CPos(3072, 0, 0), FontManager.Pixel16)
			{
				Color = Color.Yellow
			};
			score.SetText(stats.CalculateScore());

			level = new UITextLine(pos - new CPos(-1152, 0, 0), FontManager.Papyrus24);
			if (stats.Level >= stats.FinalLevel)
				level.Color = new Color(0, 200, 0);
			level.SetText(stats.Level + "/" + stats.FinalLevel);
			level.Scale = 1.4f;
		}

		public override void Render()
		{
			base.Render();
			if (Visible)
			{
				name.Scale += ContainsMouse ? 0.04f : -0.04f;

				if (name.Scale > 1.28f)
					name.Scale = 1.28f;

				if (name.Scale < 1f)
					name.Scale = 1f;

				name.Render();
				score.Render();
				level.Render();
			}
			if (Selected)
			{
				base.SetColor(new Color(1.5f, 1.5f, 1.5f));
			}
			else
			{
				base.SetColor(Color.White);
			}
		}

		public override void Tick()
		{
			base.Tick();

			if (ContainsMouse && MouseInput.IsLeftClicked)
				Selected = true;
			else if (MouseInput.IsLeftClicked)
				Selected = false;
		}
	}
}
