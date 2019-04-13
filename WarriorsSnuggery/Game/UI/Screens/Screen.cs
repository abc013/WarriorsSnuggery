using System;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI
{
	public class Screen : ITick, IRenderable, IDisposable
	{
		public readonly TextLine Title;
		public int Speed = 32;
		int scale;
		readonly Color darkness;

		public Screen(string title, int darkness = 128)
		{
			Title = new TextLine(CPos.Zero, IFont.Papyrus24, TextLine.OffsetType.MIDDLE);
			Title.SetText(title);

			this.darkness = new Color(0,0,0, darkness);
		}

		public virtual void Tick()
		{
			Title.Scale = (float) (Math.Pow(Math.Sin(scale++ / (float) Speed), 2) + 2f) / 4;
		}

		public virtual void Render()
		{
			Graphics.ColorManager.DrawFullscreenRect(darkness);
			Title.Render();
		}

		public virtual void Dispose()
		{
			Title.Dispose();
		}
	}
}
