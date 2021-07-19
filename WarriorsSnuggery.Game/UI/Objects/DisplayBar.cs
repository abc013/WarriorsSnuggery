using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI.Objects
{
	public class DisplayBar : Panel
	{
		public override CPos Position
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
		readonly UITextLine text;

		public float DisplayPercentage;

		public DisplayBar(MPos bounds, string typeName, Color fillColor) : this(bounds, PanelCache.Types[typeName], fillColor) { }

		public DisplayBar(MPos bounds, PanelType type, Color fillColor) : base(bounds, type)
		{
			this.fillColor = fillColor;

			text = new UITextLine(FontManager.Default, TextOffset.MIDDLE);
		}

		public void WriteText(string text)
		{
			this.text.WriteText(text);
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
