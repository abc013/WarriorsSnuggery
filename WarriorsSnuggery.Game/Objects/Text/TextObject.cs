using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.Objects
{
	public class TextObject : PositionableObject
	{
		public TextOffset Offset => textRenderable.Offset;
		public Font Font => textRenderable.Font;
		public string Text => textRenderable.Text;

		readonly TextRenderable textRenderable;

		public TextObject(Font font, TextOffset offset = TextOffset.LEFT)
		{
			textRenderable = new TextRenderable(font, offset);
			Renderable = textRenderable;
		}

		public void SetText(object obj) => textRenderable.SetText(obj);
		public void AddText(object obj) => textRenderable.AddText(obj);
		public void SetText(params string[] lines) => textRenderable.SetText(lines);
		public void AddText(params string[] lines) => textRenderable.AddText(lines);
	}
}
