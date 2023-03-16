using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects.Weapons;

namespace WarriorsSnuggery.Maps.Layers
{
	public sealed class SmudgeLayer
	{
		public const int MaxSmudgeCount = 256;

		public readonly List<Smudge> Smudge = new List<Smudge>();
		readonly List<Smudge> toRemove = new List<Smudge>();

		public SmudgeLayer() { }

		public void Add(Smudge smudge)
		{
			Smudge.Add(smudge);

			if (Smudge.Count > MaxSmudgeCount)
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

			foreach (var smudge in Smudge)
			{
				if (smudge.Position.X >= bottomRight.X || smudge.Position.X < topLeft.X)
					continue;

				if (smudge.Position.Y >= bottomRight.Y || smudge.Position.Y < topLeft.Y)
					continue;

				smudge.Render();
			}
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
