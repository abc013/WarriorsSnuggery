/*
 * User: Andreas
 * Date: 07.10.2018
 * Time: 16:33
 */
using System;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI
{
	public class Screen : ITick, IRenderable, IDisposable
	{
		public readonly Text Title;
		public int Speed = 32;
		int scale;
		readonly ColoredRect background;

		public Screen(string title, int darkness = 128)
		{
			Title = new Text(CPos.Zero, IFont.Papyrus24, Text.OffsetType.MIDDLE);
			Title.SetText(title);
			background = new ColoredRect(CPos.Zero, new Color(0,0,0,darkness), WindowInfo.UnitWidth);
		}

		public virtual void Tick()
		{
			Title.Scale = (float) (Math.Pow(Math.Sin(scale++ / (float) Speed), 2) + 2f) / 4;
		}

		public virtual void Render()
		{
			background.Render();
			Title.Render();
		}

		public virtual void Dispose()
		{
			background.Dispose();
			Title.Dispose();
			GC.SuppressFinalize(this);
		}
	}
}
