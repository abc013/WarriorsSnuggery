using System;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.Objects
{
	public class ActionText : TextObject
	{
		public enum ActionTextType
		{
			TRANSFORM,
			SCALE
		}

		int current;
		readonly int length;
		readonly CPos velocity;
		readonly ActionTextType type;

		public ActionText(CPos velocity, int tick, ActionTextType type, params string[] lines) : base(FontManager.Default, TextOffset.MIDDLE)
		{
			current = tick;
			length = tick;
			this.velocity = velocity;
			this.type = type;

			SetText(lines);
		}

		public override void Tick()
		{
			base.Tick();

			if (Disposed)
				return;

			if (current-- <= 0)
			{
				Dispose();
				return;
			}

			if (type == ActionTextType.TRANSFORM)
			{
				Position += velocity;
			}
			else
			{
				var time = current / (float)length - 0.75f;
				var linear = Math.Sign(time) * time;
				Scale = MathF.Pow(1 - linear, 2);
			}
		}
	}
}
