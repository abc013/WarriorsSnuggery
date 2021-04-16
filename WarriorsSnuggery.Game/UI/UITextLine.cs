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
			get => line.Color;
			set => line.Color = value;
		}

		public UITextLine(Font font, TextOffset offset = TextOffset.LEFT)
		{
			line = new TextLine(CPos.Zero, font, offset);
		}

		public void SetText(object text)
		{
			line.SetText(text);
			Bounds = line.Bounds;
		}

		public void WriteText(object text, bool add = false, bool colored = true)
		{
			line.WriteText(text, add, colored);
			Bounds = line.Bounds;
		}

		public void AddText(object text)
		{
			line.AddText(text);
			Bounds = line.Bounds;
		}

		public void SetColor(Color color)
		{
			line.Color = color;
		}

		public override void Tick()
		{
			line.Tick();
		}

		public override void Render()
		{
			line.Render();
		}

		public override void DebugRender()
		{
			var position = Position;
			if (line.Offset == TextOffset.LEFT)
				position += new CPos(Bounds.X, 0, 0);
			else if (line.Offset == TextOffset.RIGHT)
				position += new CPos(-Bounds.X, 0, 0);

			if (Bounds != MPos.Zero)
				ColorManager.DrawLineRect(position, new CPos(Bounds.X, Bounds.Y, 0), Color.Red);

			if (SelectableBounds != MPos.Zero)
				ColorManager.DrawLineRect(position, new CPos(SelectableBounds.X, SelectableBounds.Y, 0), Color.Blue);
		}
	}
}
