using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.Objects
{
	public class Smudge : PhysicsObject
	{
		readonly int maxDissolve;
		int dissolveTime;
		public Smudge(CPos pos, GraphicsObject renderable, int dissolve = 100) : base(pos, renderable)
		{
			maxDissolve = dissolve;
			dissolveTime = dissolve/2;
		}

		public override void Tick()
		{
			base.Tick();
			dissolveTime--;
			if (dissolveTime <= 0)
			{
				Renderable.SetColor(new Color(1f, 1f, 1f, 1f - dissolveTime / (-maxDissolve / 2f)));
				if (dissolveTime < -maxDissolve/2)
					Dispose();
			}
		}
	}
}
