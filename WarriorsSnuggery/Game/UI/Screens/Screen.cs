using System.Collections.Generic;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI
{
	public abstract class Screen : ITickRenderable
	{
		protected readonly TextLine Title;

		protected readonly List<ITickRenderable> Content = new List<ITickRenderable>();

		readonly Color darkness;

		public Screen(string title, int darkness = 128)
		{
			Title = new TextLine(CPos.Zero, FontManager.Papyrus24, TextLine.OffsetType.MIDDLE);
			Title.SetText(title);
			Title.Scale = 1.2f;

			this.darkness = new Color(0, 0, 0, darkness);
		}

		public virtual bool CursorOnUI()
		{
			return false;
		}

		public virtual void Show() { }

		public virtual void Hide() { }

		public virtual void Tick()
		{
			foreach (var content in Content)
				content.Tick();
		}

		public virtual void Render()
		{
			ColorManager.DrawFullscreenRect(darkness);
			Title.Render();

			foreach (var content in Content)
				content.Render();
		}
	}
}
