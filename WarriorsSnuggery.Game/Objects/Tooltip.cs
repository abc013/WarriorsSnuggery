using System.Linq;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery
{
	public class Tooltip : IRenderable
	{
		const int margin = 256;
		const int lineWidth = 16;

		public CPos Position
		{
			get => position;
			set
			{
				position = value;
				setPosition();
			}
		}
		CPos position;

		readonly Font font;
		readonly TextLine title;
		readonly TextBlock text;

		readonly MPos bounds;

		public Tooltip(CPos pos, string title, params string[] text)
		{
			font = FontManager.Pixel16;

			this.title = new TextLine(CPos.Zero, font);
			this.title.WriteText(title);
			this.text = new TextBlock(CPos.Zero, font, TextOffset.LEFT, text);
			Position = pos;

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
			Position = MouseInput.WindowPosition + new CPos(256, 0, 0);
			if (Position.X + bounds.X > WindowInfo.UnitWidth * 512)
				Position -= new CPos(bounds.X, 0, 0);

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

		void setPosition()
		{
			title.Position = position + new CPos(0, font.Height / 2, 0);
			text.Position = position + new CPos(0, 3 * font.Height / 2 + font.Gap, 0); 
		}
	}
}
