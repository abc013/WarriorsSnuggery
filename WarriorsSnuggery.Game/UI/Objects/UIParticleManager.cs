using System.Collections.Generic;

namespace WarriorsSnuggery.UI.Objects
{
	public class UIParticleManager : UIObject
	{
		readonly List<UIParticle> particles = new List<UIParticle>();

		public override void Tick()
		{
			base.Tick();

			foreach (var particle in particles)
				particle.Tick();

			particles.RemoveAll(p => p.IsDone);
		}

		public override void Render()
		{
			base.Render();

			foreach (var particle in particles)
				particle.Render();
		}

		public void Add(UIParticle particle)
		{
			particles.Add(particle);
		}
	}
}
