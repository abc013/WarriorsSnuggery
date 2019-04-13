using System;

namespace WarriorsSnuggery.Objects
{
	public class Smudge : PhysicsObject
	{
		int dissolveTime;
		public Smudge(CPos pos, GraphicsObject renderable, int dissolve = 100) : base(pos, renderable)
		{
			dissolveTime = dissolve;
		}

		public override void Tick()
		{
			base.Tick();
			dissolveTime--;
			if (dissolveTime <= 0)
			{
				Renderable.setColor(new Color(1f,1f,1f,1f - dissolveTime/-250f));
				if (dissolveTime < -250)
					Dispose();
			}
		}
	}
}
