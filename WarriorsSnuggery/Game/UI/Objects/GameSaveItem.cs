using System;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI
{
	class GameSaveItem : PanelItem
	{
		public override CPos Position
		{
			get
			{
				return base.Position;
			}
			set
			{
				base.Position = value;
				name.Position = value - new CPos(2048, 400, 0);
				level.Position = value - new CPos(2560, 0, 0);
				tooltip.Position = value;
			}
		}

		public readonly GameStatistics Stats;
		public bool Selected;

		readonly TextLine name;
		readonly TextLine level;

		readonly Tooltip tooltip;


		public GameSaveItem(CPos pos, GameStatistics stats, string renderable, int width, Action action) : base(pos, "", new ImageRenderable(TextureManager.Texture(renderable), 2f), new MPos(width, 1024), action)
		{
			Stats = stats;
			name = new TextLine(pos - new CPos(2048, 512, 0), IFont.Pixel16);
			name.SetColor(Color.Black);
			name.SetText(stats.Name);
			level = new TextLine(pos - new CPos(2560, 0, 0), IFont.Pixel16);
			level.SetColor(Color.Black);
			if (stats.Level >= stats.FinalLevel) level.SetColor(new Color(0, 200, 0));
			level.SetText(stats.Level);
			level.Scale = 2f;
			tooltip = new Tooltip(pos, stats.Name, "Difficulty: " + stats.Difficulty, "Money: " + stats.Money);
		}

		public override void Render()
		{
			base.Render();
			if (Visible)
			{
				name.Scale += mouseOnItem ? 0.04f : -0.04f;

				if (name.Scale > 1.28f)
					name.Scale = 1.28f;

				if (name.Scale < 1f)
					name.Scale = 1f;

				name.Render();
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

			if (mouseOnItem)
				UIRenderer.SetTooltip(tooltip);
			else
				UIRenderer.DisableTooltip(tooltip);

			if (mouseOnItem && MouseInput.isLeftClicked)
				Selected = true;

			else if (MouseInput.isLeftClicked)
				Selected = false;
		}

		public override void Dispose()
		{
			base.Dispose();
			name.Dispose();
			level.Dispose();
		}
	}
}
