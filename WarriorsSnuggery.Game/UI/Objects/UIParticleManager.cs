using System.Collections.Generic;

namespace WarriorsSnuggery.UI.Objects
{
	public class UIParticleManager : UIPositionable, ITick, IRenderable
	{
		readonly List<UIParticle> particles = new List<UIParticle>();

		public void Tick()
		{
			foreach (var particle in particles)
				particle.Tick();

			particles.RemoveAll(p => p.IsDone);
		}

		public void Render()
		{
			foreach (var particle in particles)
				particle.Render();
		}

		public void Add(UIParticle particle)
		{
			particles.Add(particle);
		}
	}
}
