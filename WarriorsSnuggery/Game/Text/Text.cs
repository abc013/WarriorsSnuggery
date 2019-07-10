using System;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.Objects
{
	public class Text : ITickRenderable, IPositionable, IDisposable
	{
		public CPos Position
		{
			get { return position; }
			set
			{
				position = value;

				for (int i = 0; i < lines.Length; i++)
				{
					lines[i].Position = position + new CPos(0, 1024 * i, 0);
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

				for (int i = 0; i < lines.Length; i++)
				{
					lines[i].Rotation = rotation;
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

				for (int i = 0; i < lines.Length; i++)
				{
					lines[i].Scale = scale;
				}
			}
		}
		float scale = 1f;
		readonly TextLine[] lines = new TextLine[0];

		public Text(CPos position, IFont font, TextLine.OffsetType type, params string[] args)
		{
			Position = position;
			lines = new TextLine[args.Length];

			for (int i = 0; i < args.Length; i++)
			{
				lines[i] = new TextLine(position + new CPos(0, 1024 * i, 0), font, type);
				lines[i].WriteText(args[i]);
			}
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

		public void Dispose()
		{
			foreach (var line in lines)
				line.Dispose();
		}
	}
}
