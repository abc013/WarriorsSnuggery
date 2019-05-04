/*
 * User: Andreas
 * Date: 07.10.2018
 * Time: 16:33
 */
using System;
using WarriorsSnuggery.Objects;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.UI
{
	public abstract class Screen : ITickRenderable, IDisposable
	{
		protected readonly TextLine Title;
		protected int Speed = 32;
		int scale;
		readonly Color darkness;

		public Screen(string title, int darkness = 128)
		{
			Title = new TextLine(CPos.Zero, IFont.Papyrus24, TextLine.OffsetType.MIDDLE);
			Title.SetText(title);

			this.darkness = new Color(0,0,0, darkness);
		}

		public virtual bool CursorOnUI()
		{
			return false;
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
