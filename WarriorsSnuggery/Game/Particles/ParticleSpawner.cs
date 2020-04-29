namespace WarriorsSnuggery.Objects.Particles
{
	public abstract class ParticleSpawner
	{
		public ParticleSpawner(MiniTextNode[] nodes) { }

		public abstract Particle[] Create(World world, CPos position, int height);
	}
}
