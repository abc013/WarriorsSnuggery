using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.UI
{
	public class Tooltip : IRenderable
	{
		const int margin = 128;
		const int lineWidth = 16;

		CPos position;

		readonly Font font;
		readonly UIText title;
		readonly UIText text;

		readonly MPos bounds;

		public Tooltip(string title, params string[] text)
		{
			font = FontManager.Default;

			this.title = new UIText(font);
			this.title.SetText(title);
			this.text = new UIText(font, TextOffset.LEFT);
			this.text.SetText(text);

			var width = font.Measure(this.title.Text).width;
			var textWidth = font.Measure(this.text.Text).width;

			if (textWidth > width)
				width = textWidth;

			bounds = new MPos(2 * margin + width, 2 * margin + (text.Length + 1) * font.MaxHeight + text.Length * font.HeightGap);
		}

		public void Render()
		{
			setPosition(MouseInput.WindowPosition + new CPos(256, 0, 0));

			var bottomLeft = position - new CPos(margin, margin, 0);
			var topRight = position + new CPos(bounds.X + 2 * margin, bounds.Y + 2 * margin, 0);
			ColorManager.DrawRect(bottomLeft, topRight, new Color(0, 0, 0, 0.8f));
			ColorManager.DrawFilledLineRect(bottomLeft, topRight, lineWidth, Color.White);

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
			title.Position = position + new CPos(0, font.MaxHeight, 0);
			text.Position = position + new CPos(0, 2 * (font.MaxHeight + font.HeightGap), 0);
		}
	}
}
