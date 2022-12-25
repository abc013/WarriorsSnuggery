using System;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.Objects
{
	public class ActionText : PositionableObject
	{
		public enum ActionTextType
		{
			TRANSFORM,
			SCALE
		}

		int current;
		readonly int length;
		readonly CPos velocity;
		readonly TextObject text;
		readonly ActionTextType type;

		public ActionText(CPos pos, CPos velocity, int tick, ActionTextType type, params string[] lines) : base()
		{
			Position = pos;

			current = tick;
			length = tick;
			this.velocity = velocity;
			this.type = type;

			text = new TextObject(pos, FontManager.Default, TextOffset.MIDDLE);
			text.SetText(lines);
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

			if (type == ActionTextType.TRANSFORM)
			{
				text.Position += velocity;
			}
			else
			{
				var time = current / (float)length - 0.75f;
				var linear = Math.Sign(time) * time;
				text.Scale = MathF.Pow(1 - linear, 2);
			}
		}

		public override void Render()
		{
			text.Render();
		}
	}
}
