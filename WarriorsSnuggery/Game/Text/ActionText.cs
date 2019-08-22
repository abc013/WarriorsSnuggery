using System;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.Objects
{
	public class ActionText : PhysicsObject
	{
		public enum ActionTextType
		{
			TRANSFORM,
			SCALE
		}

		int current;
		readonly int length;
		readonly CPos velocity;
		readonly TextBlock text;
		readonly ActionTextType type;

		public ActionText(CPos pos, CPos velocity, int tick, ActionTextType type, params string[] lines) : base(pos)
		{
			current = tick;
			length = tick;
			this.velocity = velocity;
			this.type = type;

			text = new TextBlock(pos, IFont.Pixel16, TextLine.OffsetType.MIDDLE, lines);
			WorldRenderer.RenderAfter(text);
		}

		public override void Tick()
		{
			if (Disposed)
				return;

			if (current-- <= 0)
			{
				Dispose();
				WorldRenderer.RemoveRenderAfter(text);
				return;
			}

			if (type == ActionTextType.TRANSFORM)
			{
				text.Position += velocity;
			}
			else
			{
				var time = current / (float)length - 0.75f;
				var linear = Math.Sign(time) * time;
				text.Scale = (float)Math.Pow(1 - linear, 2);
			}
		}
	}
}
