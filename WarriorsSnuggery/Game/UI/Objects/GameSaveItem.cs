using System;
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
				name.Position = value - new CPos(2048, 400,0);
				level.Position = value - new CPos(2560, 0, 0);
			}
		}

		public bool Selected;
		readonly Text name;
		readonly Text level;

		public readonly GameStatistics Stats;

		public GameSaveItem(CPos pos, GameStatistics stats, string renderable, int width, Action action) : base(pos, stats.Name, new ImageRenderable(TextureManager.Texture(renderable), 2f), new MPos(width, 1024), action)
		{
			Stats = stats;
			name = new Text(pos - new CPos(2048, 512, 0), IFont.Pixel16);
			name.SetColor(Color.Black);
			name.SetText(stats.Name);
			level = new Text(pos - new CPos(2560, 0, 0), IFont.Pixel16);
			level.SetColor(Color.Black);
			if (stats.Level >= stats.FinalLevel) level.SetColor(new Color(0,200,0));
			level.SetText(stats.Level);
			level.Scale = 2f;
		}

		public override void Render()
		{
			base.Render();
			name.Scale += mouseOnItem ? 0.04f : -0.04f;

			if (name.Scale > 1.28f)
				name.Scale = 1.28f;

			if (name.Scale < 1f)
				name.Scale = 1f;

			name.Render();
			level.Render();
		}

		public override void Tick()
		{
			base.Tick();

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
