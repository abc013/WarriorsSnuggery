using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.Objects.Weapons
{
	public class Smudge : ITickRenderable
	{
		public readonly CPos Position;
		readonly BatchRenderable renderable;


		public bool IsDissolving { get; private set; }
		public bool Dissolved;

		readonly int dissolveDuration;
		int currentDissolve;

		public Smudge(CPos pos, BatchRenderable renderable, int dissolveDuration, bool instantDissolve = false)
		{
			this.renderable = renderable;
			Position = pos;
			renderable.SetPosition(Position);

			this.dissolveDuration = dissolveDuration;
			currentDissolve = dissolveDuration;

			IsDissolving = instantDissolve;
		}

		public void Tick()
		{
			if (Dissolved)
				return;

			renderable.Tick();
			if (IsDissolving)
			{
				if (currentDissolve-- > 0)
					renderable.SetColor(Color.White.WithAlpha(currentDissolve / (float)dissolveDuration));
				else
					Dissolved = true;
			}
		}

		public void Render()
		{
			if (Dissolved)
				return;

			renderable.Render();
		}

		public void BeginDissolve()
		{
			IsDissolving = true;
		}
	}
}
