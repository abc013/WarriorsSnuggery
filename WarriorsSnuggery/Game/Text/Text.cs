using System;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.Objects
{
	public class TextBlock : ITickRenderable, IPositionable, IDisposable
	{
		public CPos Position
		{
			get { return position; }
			set
			{
				position = value;

				for (int i = 0; i < Lines.Length; i++)
				{
					Lines[i].Position = position + new CPos(0, lineDistance * i, 0);
				}
			}
		}
		CPos position;

		public VAngle Rotation
		{
			get { return rotation; }
			set
			{
				rotation = value;

				for (int i = 0; i < Lines.Length; i++)
				{
					Lines[i].Rotation = rotation;
				}
			}
		}
		VAngle rotation;

		public float Scale
		{
			get { return scale; }
			set
			{
				scale = value;

				for (int i = 0; i < Lines.Length; i++)
				{
					Lines[i].Scale = scale;
				}
			}
		}
		float scale = 1f;

		public readonly TextLine[] Lines = new TextLine[0];
		readonly int lineDistance;

		public TextBlock(CPos position, IFont font, TextLine.OffsetType type, params string[] text)
		{
			Position = position;
			Lines = new TextLine[text.Length];

			lineDistance = font == IFont.Pixel16 ? 512 : 1024;
			for (int i = 0; i < text.Length; i++)
			{
				Lines[i] = new TextLine(position + new CPos(0, lineDistance * i, 0), font, type); // TODO what if other fonts will occur?
				Lines[i].WriteText(text[i]);
			}
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

		public void Dispose()
		{
			foreach (var line in Lines)
				line.Dispose();
		}
	}
}
