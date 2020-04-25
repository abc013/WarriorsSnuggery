using System.Linq;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery
{
	public class Tooltip : IRenderable
	{
		const int margin = 64;

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

		readonly Font font;
		readonly TextLine title;
		readonly TextBlock text;

		readonly CPos size;

		public Tooltip(CPos pos, string title, params string[] text)
		{
			font = Font.Pixel16;

			this.title = new TextLine(CPos.Zero, font);
			this.title.WriteText(title);
			this.text = new TextBlock(CPos.Zero, font, TextLine.OffsetType.LEFT, text);
			Position = pos;

			var xChars = this.title.String.Length;
			if (text.Length != 0)
			{
				var maxInText = this.text.Lines.Max(s => s.String.Length);

				if (maxInText > xChars)
					xChars = maxInText;
			}

			size = new CPos((xChars + 1) * font.Width + 2 * margin, (text.Length + 1) * font.Height + 2 * margin, 0);
		}

		public void Render()
		{
			Position = MouseInput.WindowPosition + new CPos(256, 0, 0);
			if (Position.X + size.X > WindowInfo.UnitWidth * 512)
				Position -= new CPos(size.X, 0, 0);

			ColorManager.DrawRect(position - new CPos(-margin, margin, 0), position + size, new Color(0, 0, 0, 0.8f));
			ColorManager.LineWidth = 3f;
			ColorManager.DrawDot(position, Color.White);
			ColorManager.DrawDot(position + new CPos(0, size.Y, 0), Color.White);
			ColorManager.DrawDot(position + new CPos(size.X, 0, 0), Color.White);
			ColorManager.DrawDot(position + new CPos(size.X, size.Y, 0), Color.White);
			ColorManager.ResetLineWidth();
			title.Render();
			text.Render();
		}

		void setPosition()
		{
			title.Position = position + new CPos(font.Width, font.Height / 2, 0);
			text.Position = position + new CPos(font.Width, font.Height / 2 * 3, 0);
		}
	}
}
