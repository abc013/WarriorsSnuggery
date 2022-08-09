using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.UI
{
	public class Tooltip : IRenderable
	{
		const int margin = 128;
		const int lineWidth = 16;

		UIPos position;

		readonly Font font;
		readonly UIText title;
		readonly UIText text;

		readonly UIPos bounds;

		public Tooltip(string title, params string[] text)
		{
			font = FontManager.Default;

			this.title = new UIText(font);
			this.title.SetText(title);
			this.title.Scale = 1.1f;
			this.text = new UIText(font, TextOffset.LEFT);
			this.text.SetText(text);

			var width = (font.Measure(this.title.Text).width * 11) / 10;
			var textWidth = font.Measure(this.text.Text).width;

			if (textWidth > width)
				width = textWidth;

			bounds = new UIPos(2 * margin + width, 2 * margin + (text.Length + 1) * font.MaxHeight + text.Length * font.HeightGap);
		}

		public void Render()
		{
			setPosition(MouseInput.WindowPosition + new UIPos(256, 0));

			var bottomLeft = position - new UIPos(margin, margin);
			var topRight = position + new UIPos(bounds.X + 2 * margin, bounds.Y + 2 * margin);
			ColorManager.DrawRect(bottomLeft, topRight, new Color(0, 0, 0, 0.8f));
			ColorManager.DrawFilledLineRect(bottomLeft, topRight, lineWidth, Color.White);

			title.Render();
			text.Render();
		}

		void setPosition(UIPos pos)
		{
			var posX = pos.X + bounds.X + margin;
			if (posX > WindowInfo.UnitWidth * 512)
				pos -= new UIPos(posX - ((int)WindowInfo.UnitWidth * 512), 0);

			var posY = pos.Y + bounds.Y + margin;
			if (posY > WindowInfo.UnitHeight * 512)
				pos -= new UIPos(0, posY - ((int)WindowInfo.UnitHeight * 512));

			position = pos;
			title.Position = position + new UIPos(0, font.MaxHeight);
			text.Position = position + new UIPos(0, 2 * font.MaxHeight + font.HeightGap);
		}
	}
}
