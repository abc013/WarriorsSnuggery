using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.Objects
{
	public class TextObject : IPositionable, ITickRenderable
	{
		readonly TextRenderable text;

		public TextOffset Offset => text.Offset;
		public Font Font => text.Font;
		public string Text => text.Text;

		public virtual CPos Position
		{
			get => position;
			set
			{
				position = value;

				text.SetPosition(position);
			}
		}
		CPos position;

		public virtual VAngle Rotation
		{
			get => rotation;
			set
			{
				rotation = value;

				text.SetRotation(rotation);
			}
		}
		VAngle rotation;

		public virtual float Scale
		{
			get => scale;
			set
			{
				scale = value;

				text.SetScale(scale);
			}
		}
		float scale = 1f;

		public Color Color
		{
			get => color;
			set
			{
				color = value;

				text.SetColor(color);
			}
		}
		Color color = Color.White;

		public TextObject(CPos pos, Font font, TextOffset offset = TextOffset.LEFT)
		{
			text = new TextRenderable(font, offset);

			Position = pos;
		}

		public void SetText(object obj) => text.SetText(obj);
		public void AddText(object obj) => text.AddText(obj);
		public void SetText(params string[] lines) => text.SetText(lines);
		public void AddText(params string[] lines) => text.AddText(lines);

		public void Tick() => text.Tick();
		public void Render() => text.Render();
	}
}
