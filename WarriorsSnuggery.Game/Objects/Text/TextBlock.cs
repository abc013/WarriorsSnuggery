using System.Linq;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.Objects
{
	public class TextBlock : IPositionable, ITickRenderable
	{
		public CPos Position
		{
			get => position;
			set
			{
				position = value;

				for (int i = 0; i < Lines.Length; i++)
					Lines[i].Position = position + new CPos(0, (font.HeightGap + font.MaxHeight / 2) * i, 0);
			}
		}
		CPos position;

		public VAngle Rotation
		{
			get => rotation;
			set
			{
				rotation = value;

				for (int i = 0; i < Lines.Length; i++)
					Lines[i].Rotation = rotation;
			}
		}
		VAngle rotation;

		public float Scale
		{
			get => scale;
			set
			{
				scale = value;

				for (int i = 0; i < Lines.Length; i++)
					Lines[i].Scale = scale;
			}
		}
		float scale = 1f;

		readonly Font font;
		public readonly TextLine[] Lines;

		public MPos Bounds { get; private set; }

		public TextBlock(CPos position, Font font, TextOffset offset, params string[] text)
		{
			this.font = font;
			Lines = new TextLine[text.Length];

			for (int i = 0; i < text.Length; i++)
			{
				Lines[i] = new TextLine(position, font, offset);
				Lines[i].WriteText(text[i]);
			}
			Position = position;

			var width = Lines.Length == 0 ? 0 : Lines.Max(s => font.GetWidth(s.Text));

			Bounds = new MPos(width, text.Length * (font.MaxHeight + font.HeightGap) / 2);
		}

		public void Render()
		{
			foreach (var line in Lines)
				line.Render();
		}

		public void Tick()
		{
			foreach (var line in Lines)
				line.Tick();
		}
	}
}
