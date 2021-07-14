using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Objects.Weapons;

namespace WarriorsSnuggery.Maps.Layers
{
	public sealed class SmudgeLayer
	{
		public readonly List<Smudge> Smudge = new List<Smudge>();
		readonly List<Smudge> visibleSmudge = new List<Smudge>();
		readonly List<Smudge> toRemove = new List<Smudge>();

		public SmudgeLayer() { }

		public void Add(Smudge smudge)
		{
			Smudge.Add(smudge);
			if (smudge.CheckVisibility())
				visibleSmudge.Add(smudge);

			if (Smudge.Count > 256)
			{
				for (int i = 0; i < Smudge.Count; i++)
				{
					if (!Smudge[i].IsDissolving)
					{
						Smudge[i].BeginDissolve();
						break;
					}
				}
			}
		}

		public void Render()
		{
			foreach (var smudge in visibleSmudge)
				smudge.Render();
		}

		public void Tick()
		{
			foreach (var smudge in Smudge)
			{
				smudge.Tick();
				if (smudge.Dissolved)
					toRemove.Add(smudge);
			}

			if (toRemove.Count != 0)
			{
				foreach (var smudge in toRemove)
				{
					Smudge.Remove(smudge);
					visibleSmudge.Remove(smudge);
				}
				toRemove.Clear();
			}
		}

		public void CheckVisibility()
		{
			foreach (var s in Smudge)
				s.CheckVisibility();
			visibleSmudge.Clear();
			visibleSmudge.AddRange(Smudge);
		}

		public void CheckVisibility(CPos topLeft, CPos bottomRight)
		{
			visibleSmudge.Clear();

			foreach (var w in Smudge.Where(a => a.Position.X > topLeft.X && a.Position.X < bottomRight.X && a.Position.Y > topLeft.Y && a.Position.Y < bottomRight.Y))
			{
				if (w.CheckVisibility())
					visibleSmudge.Add(w);
			}
		}

		public void Clear()
		{
			foreach (var smudge in Smudge)
				smudge.Dissolved = true;
			Smudge.Clear();
			visibleSmudge.Clear();
		}
	}
}
