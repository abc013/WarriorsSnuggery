using System.Collections.Generic;

namespace WarriorsSnuggery.UI
{
	public class SquareParticleManager : UIObject
	{
		readonly List<SquareParticle> particles = new List<SquareParticle>();

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

		public SquareParticle Add(int duration)
		{
			var particle = new SquareParticle(duration);
			particles.Add(particle);

			return particle;
		}
	}
}
