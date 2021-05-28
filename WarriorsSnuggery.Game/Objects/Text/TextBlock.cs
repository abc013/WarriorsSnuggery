using System.Collections.Generic;
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

				for (int i = 0; i < lines.Count; i++)
					lines[i].Position = position + new CPos(0, (font.HeightGap / 2 + font.MaxHeight / 2) * i, 0);
			}
		}
		CPos position;

		public VAngle Rotation
		{
			get => rotation;
			set
			{
				rotation = value;

				for (int i = 0; i < lines.Count; i++)
					lines[i].Rotation = rotation;
			}
		}
		VAngle rotation;

		public float Scale
		{
			get => scale;
			set
			{
				scale = value;

				for (int i = 0; i < lines.Count; i++)
					lines[i].Scale = scale;
			}
		}
		float scale = 1f;

		public MPos Bounds { get; private set; }

		readonly Font font;
		readonly TextOffset offset;

		readonly List<TextLine> lines = new List<TextLine>();
		public TextLine this[int index] => lines[index];
		public int LineCount => lines.Count;

		public TextBlock(CPos position, Font font, TextOffset offset = TextOffset.LEFT)
		{
			Position = position;

			this.font = font;
			this.offset = offset;
		}

		public void Clear()
		{
			lines.Clear();

			calculateBounds();
		}

		public void Add(params string[] text)
		{
			foreach (var line in text)
				add(line);

			calculateBounds();
		}

		void add(string text)
		{
			var line = new TextLine(position, font, offset);
			line.WriteText(text);

			lines.Add(line);
		}

		public void RemoveLast()
		{
			lines.Remove(lines[^1]);

			calculateBounds();
		}

		void calculateBounds()
		{
			var width = lines.Count == 0 ? 0 : lines.Max(s => font.GetWidth(s.Text));

			Bounds = new MPos(width, lines.Count * (font.MaxHeight + font.HeightGap) / 2);
		}

		public void Render()
		{
			foreach (var line in lines)
				line.Render();
		}

		public void Tick()
		{
			foreach (var line in lines)
				line.Tick();
		}
	}
}
