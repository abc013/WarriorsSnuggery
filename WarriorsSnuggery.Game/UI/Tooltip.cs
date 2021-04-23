﻿using System.Linq;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI
{
	public class Tooltip : IRenderable
	{
		const int margin = 256;
		const int lineWidth = 16;

		CPos position;

		readonly Font font;
		readonly TextLine title;
		readonly TextBlock text;

		readonly MPos bounds;

		public Tooltip(string title, params string[] text)
		{
			font = FontManager.Pixel16;

			this.title = new TextLine(CPos.Zero, font);
			this.title.WriteText(title);
			this.text = new TextBlock(CPos.Zero, font, TextOffset.LEFT, text);

			var width = font.GetWidth(this.title.Text);
			if (text.Length != 0)
			{
				var textWidth = this.text.Lines.Max(s => font.GetWidth(s.Text));

				if (textWidth > width)
					width = textWidth;
			}

			bounds = new MPos(width * 2, text.Length * (font.Height + font.Gap));
		}

		public void Render()
		{
			setPosition(MouseInput.WindowPosition + new CPos(256, 0, 0));

			ColorManager.DrawRect(position - new CPos(margin, margin, 0), position + new CPos(bounds.X + 2 * margin, bounds.Y + 2 * margin, 0), new Color(0, 0, 0, 0.8f));
			ColorManager.LineWidth = 3f;
			ColorManager.DrawRect(position - new CPos(margin + lineWidth, margin + lineWidth, 0), position - new CPos(margin - lineWidth, -(bounds.Y + 2 * margin + lineWidth), 0), Color.White);
			ColorManager.DrawRect(position - new CPos(margin + lineWidth, margin + lineWidth, 0), position - new CPos(-(bounds.X + 2 * margin + lineWidth), margin - lineWidth, 0), Color.White);
			ColorManager.DrawRect(position - new CPos(margin + lineWidth, -(bounds.Y + 2 * margin + lineWidth), 0), position - new CPos(-(bounds.X + 2 * margin + lineWidth), -(bounds.Y + 2 * margin - lineWidth), 0), Color.White);
			ColorManager.DrawRect(position - new CPos(-(bounds.X + 2 * margin + lineWidth), margin + lineWidth, 0), position - new CPos(-(bounds.X + 2 * margin - lineWidth), -(bounds.Y + 2 * margin + lineWidth), 0), Color.White);
			ColorManager.ResetLineWidth();
			title.Render();
			text.Render();
		}

		void setPosition(CPos pos)
		{
			if (pos.X + bounds.X + margin > WindowInfo.UnitWidth * 512)
				pos -= new CPos(bounds.X, 0, 0);
			if (pos.Y + bounds.Y + margin > WindowInfo.UnitHeight * 512)
				pos -= new CPos(0, bounds.Y, 0);

			position = pos;
			title.Position = position + new CPos(0, font.Height / 2, 0);
			text.Position = position + new CPos(0, 3 * font.Height / 2 + font.Gap, 0);
		}
	}
}