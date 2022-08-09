using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.UI.Objects
{
	public class UIParticle : UIPositionable, ITick, IRenderable
	{
		public bool IsDone => tick <= 0;
		readonly int duration;
		int tick;

		public Color Color;
		public UIPos Velocity;
		public UIPos Force;

		public int Radius;

		public UIParticle(int duration)
		{
			this.duration = duration;
			tick = duration;
		}

		public void Tick()
		{
			Position += Velocity;
			Velocity += Force;

			tick--;
		}

		public void Render()
		{
			ColorManager.DrawQuad(Position, Radius, Color.WithAlpha(tick/(float)duration));
		}
	}
}
