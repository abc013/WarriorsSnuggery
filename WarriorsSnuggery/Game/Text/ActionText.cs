using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.Objects
{
	public class ActionText : PhysicsObject
	{
		int current;
		readonly int start;
		readonly CPos velocity;
		readonly Text text;

		public ActionText(CPos pos, IFont font, CPos velocity, int tick, params string[] lines) : base(pos)
		{
			current = tick;
			start = tick;
			this.velocity = velocity;

			text = new Text(pos, IFont.Pixel16, TextLine.OffsetType.MIDDLE, lines);
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

			text.Position += velocity;
		}

		public override void Render()
		{
			text.Render();
		}
	}
}
