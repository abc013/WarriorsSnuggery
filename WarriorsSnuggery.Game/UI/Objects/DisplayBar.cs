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

		public override VAngle Rotation
		{
			get => base.Rotation;
			set
			{
				base.Rotation = value;
				text.Rotation = value;
			}
		}

		public override float Scale
		{
			get => base.Scale;
			set
			{
				base.Scale = value;
				text.Scale = value;
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

			var offset = Position - new CPos(SelectableBounds.X, SelectableBounds.Y, 0);
			ColorManager.DrawRect(offset, offset + new CPos((int)(2 * SelectableBounds.X * DisplayPercentage), 2 * SelectableBounds.Y, 0), fillColor);

			text.Render();
		}
	}
}
