using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.Objects
{
	public class Smudge : PhysicsObject
	{
		public bool IsDissolving;

		readonly int maxDissolve;
		int dissolveTime;

		public Smudge(CPos pos, GraphicsObject renderable, int dissolve) : base(pos, renderable)
		{
			maxDissolve = dissolve;
		}

		public override void Tick()
		{
			base.Tick();
			if (IsDissolving)
			{
				if (dissolveTime-- > 0)
					Renderable.SetColor(new Color(1f, 1f, 1f, dissolveTime / (float)maxDissolve));
				else
					Dispose();
			}
		}

		public void BeginDissolve()
		{
			IsDissolving = true;
			dissolveTime = maxDissolve;
		}
	}
}
