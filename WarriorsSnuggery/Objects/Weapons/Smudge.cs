using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.Objects.Weapons
{
	public class Smudge : PositionableObject
	{
		public bool IsDissolving;

		readonly int maxDissolve;
		int dissolveTime;

		public Smudge(CPos pos, BatchRenderable renderable, int dissolve, bool instantDissolve = false) : base(pos, renderable)
		{
			maxDissolve = dissolve;
			if (instantDissolve)
				BeginDissolve();
		}

		public override void Tick()
		{
			base.Tick();
			Renderable?.Tick();
			if (IsDissolving)
			{
				if (dissolveTime-- > 0)
					Renderable.SetColor(new Color(1f, 1f, 1f, dissolveTime / (float)maxDissolve));
				else
					Disposed = true;
			}
		}

		public void BeginDissolve()
		{
			IsDissolving = true;
			dissolveTime = maxDissolve;
		}
	}
}
