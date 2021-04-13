using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI
{
	public class DisplayBar : Panel
	{
		public override CPos Position
		{
			get => base.Position;
			set
			{
				base.Position = value;

				if (text != null)
					text.Position = value;
			}
		}

		public override VAngle Rotation
		{
			get => base.Rotation;
			set
			{
				base.Rotation = value;

				if (text != null)
					text.Rotation = value;
			}
		}

		public override float Scale
		{
			get => base.Scale;
			set
			{
				base.Scale = value;

				if (text != null)
					text.Scale = value;
			}
		}

		readonly Color fillColor;
		readonly UITextLine text;

		public float DisplayPercentage;

		public DisplayBar(CPos pos, MPos bounds, PanelType type, Color fillColor) : base(pos, bounds, type)
		{
			this.fillColor = fillColor;

			text = new UITextLine(pos, FontManager.Pixel16, TextOffset.MIDDLE);
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
