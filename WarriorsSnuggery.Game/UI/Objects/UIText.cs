﻿using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.UI
{
	public class UIText : UIPositionable, ITick, IRenderable
	{
		readonly TextRenderable text;

		public TextOffset Offset => text.Offset;
		public Font Font => text.Font;
		public string Text => text.Text;

		public override UIPos Position
		{
			get => base.Position;
			set
			{
				base.Position = value;
				text.SetPosition(value);
			}
		}

		public float Scale
		{
			get => scale;
			set
			{
				scale = value;
				text.SetScale(value);
			}
		}
		float scale;

		public Color Color
		{
			get => color;
			set
			{
				color = value;
				text.SetColor(value);
			}
		}
		Color color;

		public UIText(Font font, TextOffset offset = TextOffset.LEFT)
		{
			text = new TextRenderable(font, offset);
		}

		public UIText(Font font, TextOffset offset, params string[] lines) : this(font, offset)
		{
			text.SetText(lines);
		}

		public void SetText(object obj, bool interpretColors = true)
		{
			text.SetText(obj, interpretColors);
			recalculateBounds();
		}

		public void AddText(object obj, bool interpretColors = true)
		{
			text.AddText(obj, interpretColors);
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

			Bounds = new UIPos(width / 2, height);
		}

		public virtual void Tick() => text.Tick();
		public virtual void Render() => text.Render();

		public override void DebugRender()
		{
			var position = Position;
			if (text.Offset == TextOffset.LEFT)
				position += new UIPos(Bounds.X, 0);
			else if (text.Offset == TextOffset.RIGHT)
				position -= new UIPos(Bounds.X, 0);

			if (!Bounds.IsEmpty())
				ColorManager.DrawLineQuad(position, Bounds, Color.Red);

			if (!SelectableBounds.IsEmpty())
				ColorManager.DrawLineQuad(position, SelectableBounds, Color.Blue);
		}
	}
}
