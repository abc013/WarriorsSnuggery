using System;

namespace WarriorsSnuggery.Objects
{
	public class ActionText : TextLine
	{
		int current;
		readonly int start;
		readonly CPos velocity;

		public ActionText(CPos pos, string text, Color color, IFont font, CPos velocity, int tick = 100) : base(pos, font, OffsetType.MIDDLE)
		{
			current = tick;
			start = tick;
			this.velocity = velocity;

			SetColor(color);
			SetText(text);
		}

		public override void Tick()
		{
			if (Disposed)
				return;

			if (current-- <= 0)
			{
				Dispose();
				return;
			}

			SetColor(new Color(color.R, color.G, color.B, (float) Math.Sqrt(current / (float) start)));
			SetText(String);

			Position += velocity;
		}
	}
}
