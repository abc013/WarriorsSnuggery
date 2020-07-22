using System;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

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

		readonly TextLine name;
		readonly TextLine score;
		readonly TextLine level;

		public GameSaveItem(CPos pos, GameStatistics stats, int width, Action action) : base(pos, new BatchObject(UITextureManager.Get("UI_save")[0], Color.White), new MPos(width, 1024), stats.Name, new[] { Color.Grey + "Difficulty: " + stats.Difficulty, Color.Grey + "Money: " + stats.Money }, action)
		{
			Stats = stats;
			Scale *= 2;
			
			name = new TextLine(pos - new CPos(3072, 512, 0), FontManager.Pixel16);
			name.SetText(stats.Name);

			score = new TextLine(pos - new CPos(3072, 0, 0), FontManager.Pixel16);
			score.SetColor(Color.Yellow);
			score.SetText(stats.CalculateScore());

			level = new TextLine(pos - new CPos(-1152, 0, 0), FontManager.Papyrus24);
			if (stats.Level >= stats.FinalLevel)
				level.SetColor(new Color(0, 200, 0));
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
