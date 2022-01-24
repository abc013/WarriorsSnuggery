using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.UI
{
	public class UIText : UIObject
	{
		readonly TextRenderable text;

		public TextOffset Offset => text.Offset;
		public Font Font => text.Font;
		public string Text => text.Text;

		public override CPos Position
		{
			get => base.Position;
			set
			{
				base.Position = value;

				text.SetPosition(value);
			}
		}

		public override VAngle Rotation
		{
			get => base.Rotation;
			set
			{
				base.Rotation = value;

				text.SetRotation(value);
			}
		}

		public override float Scale
		{
			get => base.Scale;
			set
			{
				base.Scale = value;

				text.SetScale(value);
			}
		}

		public override Color Color
		{
			get => base.Color;
			set
			{
				base.Color = value;

				text.SetColor(value);
			}
		}

		public UIText(Font font, TextOffset offset = TextOffset.LEFT)
		{
			text = new TextRenderable(font, offset);
		}

		public UIText(Font font, TextOffset offset, params string[] lines) : this(font, offset)
		{
			text.SetText(lines);
		}

		public void SetText(object obj)
		{
			text.SetText(obj);
			recalculateBounds();
		}

		public void AddText(object obj)
		{
			text.AddText(obj);
			recalculateBounds();
		}

		public void SetText(params string[] lines)
		{
			text.SetText(lines);
			recalculateBounds();
		}

		public void AddText(params string[] lines)
		{
			text.AddText(lines);
			recalculateBounds();
		}

		void recalculateBounds()
		{
			var (width, height) = Font.Measure(Text);

			Bounds = new MPos(width / 2, height);
		}

		public override void Tick() => text.Tick();
		public override void Render() => text.Render();

		public override void DebugRender()
		{
			var position = Position;
			if (text.Offset == TextOffset.LEFT)
				position += new CPos(Bounds.X, 0, 0);
			else if (text.Offset == TextOffset.RIGHT)
				position -= new CPos(Bounds.X, 0, 0);

			if (Bounds != MPos.Zero)
				ColorManager.DrawLineQuad(position, new CPos(Bounds.X, Bounds.Y, 0), Color.Red);

			if (SelectableBounds != MPos.Zero)
				ColorManager.DrawLineQuad(position, new CPos(SelectableBounds.X, SelectableBounds.Y, 0), Color.Blue);
		}
	}
}
