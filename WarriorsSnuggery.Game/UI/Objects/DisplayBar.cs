using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.UI.Objects
{
	public class DisplayBar : Panel
	{
		public override UIPos Position
		{
			get => base.Position;
			set
			{
				base.Position = value;
				text.Position = value;
			}
		}

		readonly Color fillColor;
		readonly UIText text;

		public float DisplayPercentage;

		public DisplayBar(UIPos bounds, string typeName, Color fillColor) : this(bounds, PanelCache.Types[typeName], fillColor) { }

		public DisplayBar(UIPos bounds, PanelType type, Color fillColor) : base(bounds, type)
		{
			this.fillColor = fillColor;

			text = new UIText(FontManager.Default, TextOffset.MIDDLE);
		}

		public void SetText(string text)
		{
			this.text.SetText(text);
		}

		public override void Render()
		{
			base.Render();

			var offset = Position - SelectableBounds;
			ColorManager.DrawRect(offset, offset + new UIPos((int)(2 * SelectableBounds.X * DisplayPercentage), 2 * SelectableBounds.Y), fillColor);

			text.Render();
		}
	}
}
