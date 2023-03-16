using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects.Weapons;

namespace WarriorsSnuggery.Maps.Layers
{
	public sealed class SmudgeLayer
	{
		public readonly List<Smudge> Smudge = new List<Smudge>();
		readonly List<Smudge> toRemove = new List<Smudge>();

		public SmudgeLayer() { }

		public void Add(Smudge smudge)
		{
			Smudge.Add(smudge);

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
			CameraVisibility.GetClampedBounds(out var pos, out var bounds);
			var topLeft = pos.ToCPos();
			var bottomRight = pos.ToCPos() + bounds.ToCPos();

			foreach (var smudge in Smudge.Where(a => a.CheckVisibility()))
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
					Smudge.Remove(smudge);
				toRemove.Clear();
			}
		}
	}
}
