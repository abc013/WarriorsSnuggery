namespace WarriorsSnuggery.Objects.Particles
{
	[Desc("Spawns all particles at one point.")]
	public class PointParticleSpawner : ParticleSpawner
	{
		[Desc("Type of particle to spawn.")]
		public readonly ParticleType Type;

		[Desc("Particle count.")]
		public readonly int Count = 1;

		[Desc("Offset of the spawning point.")]
		public readonly CPos Offset = CPos.Zero;

		public PointParticleSpawner(MiniTextNode[] nodes) : base()
		{
			Loader.PartLoader.SetValues(this, nodes);
		}

		public override Particle[] Create(World world, CPos position, int height)
		{
			var particles = new Particle[Count];
			for (int i = 0; i < Count; i++)
				particles[i] = ParticleCreator.Create(world, Type, position + Offset, height);

			return particles;
		}
	}
}
