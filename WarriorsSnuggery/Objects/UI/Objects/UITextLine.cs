using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI
{
	public class UITextLine : UIObject
	{
		readonly TextLine line;

		public override CPos Position
		{
			get => line.Position;
			set => line.Position = value;
		}

		public override float Scale
		{
			get => line.Scale;
			set => line.Scale = value;
		}

		public override VAngle Rotation
		{
			get => line.Rotation;
			set => line.Rotation = value;
		}

		public override Color Color
		{
			get => Color.White;
			set => line.SetColor(value);
		}

		public UITextLine(CPos pos, Font font, TextOffset offset = TextOffset.LEFT)
		{
			line = new TextLine(pos, font, offset);
		}

		public void SetText(object text)
		{
			line.SetText(text);
		}

		public void WriteText(object text, bool add = false, bool colored = true)
		{
			line.WriteText(text, add, colored);
		}

		public void AddText(object text)
		{
			line.AddText(text);
		}

		public void SetColor(Color color)
		{
			line.SetColor(color);
		}

		public override void Tick()
		{
			line.Tick();
		}

		public override void Render()
		{
			line.Render();
		}
	}
}
