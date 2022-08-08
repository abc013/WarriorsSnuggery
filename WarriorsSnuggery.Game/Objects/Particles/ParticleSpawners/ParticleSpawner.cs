using System.Collections.Generic;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Objects.Particles
{
	public abstract class ParticleSpawner
	{
		public ParticleSpawner(List<TextNode> nodes)
		{
			TypeLoader.SetValues(this, nodes);
		}

		public abstract Particle[] Create(World world, CPos position);
	}
}
