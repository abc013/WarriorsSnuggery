namespace WarriorsSnuggery.Objects.Particles
{
	public class ParticleSpawner
	{
		readonly ParticleType type;
		readonly int count;
		readonly int radius;

		public ParticleSpawner(ParticleType type, int count, int radius)
		{
			this.type = type;
			this.count = count;
			this.radius = radius;
		}

		public Particle[] Create(World world, CPos position, int height)
		{
			var particles = new Particle[count];
			for (int i = 0; i < count; i++)
			{
				var ranX = Program.SharedRandom.Next(radius * 2) - radius;
				var ranY = Program.SharedRandom.Next(radius * 2) - radius;
				var pos = new CPos(ranX, ranY, 0);

				particles[i] = ParticleCreator.Create(type, position + pos, height, world.Game.SharedRandom);
			}
			return particles;
		}
	}
}
