﻿using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.UI
{
	public class SquareParticle : UIObject
	{
		public bool IsDone => tick <= 0;
		readonly int duration;
		int tick;

		public CPos Velocity;
		public CPos Force;

		public int Radius;

		public SquareParticle(int duration)
		{
			this.duration = duration;
			tick = duration;
		}

		public override void Tick()
		{
			Position += Velocity;
			Velocity += Force;

			tick--;
		}

		public override void Render()
		{
			ColorManager.DrawQuad(Position, Radius, Color * new Color(1f, 1f, 1f, tick/(float)duration));
		}
	}
}
