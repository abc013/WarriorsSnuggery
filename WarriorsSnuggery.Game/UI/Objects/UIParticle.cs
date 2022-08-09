using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.UI.Objects
{
	public class UIParticle : UIObject
	{
		public bool IsDone => tick <= 0;
		readonly int duration;
		int tick;

		public UIPos Velocity;
		public UIPos Force;

		public int Radius;

		public UIParticle(int duration)
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
			ColorManager.DrawQuad(Position, Radius, Color.WithAlpha(tick/(float)duration));
		}
	}
}
