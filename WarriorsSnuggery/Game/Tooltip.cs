using System;
using WarriorsSnuggery.Objects;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery
{
	public class Tooltip : IRenderable
	{
		public CPos Position
		{
			get
			{
				return position;
			}
			set
			{
				position = value;
				setPosition();
			}
		}
		CPos position;

		readonly TextLine title;
		readonly TextBlock text;

		public Tooltip(CPos pos, string title, params string[] text)
		{
			this.title = new TextLine(pos, IFont.Pixel16);
			this.title.WriteText(title);
			this.text = new TextBlock(pos + new CPos(0, 1024, 0), IFont.Pixel16, TextLine.OffsetType.LEFT, text);
			Position = pos;
		}

		public void Render()
		{
			ColorManager.DrawRect(position, position + new CPos(4096, 2048, 0), new Color(0,0,0,0.5f));
			title.Render();
			text.Render();
		}

		void setPosition()
		{
			title.Position = position;
			text.Position = position + new CPos(0, 1024, 0);
		}
	}
}
