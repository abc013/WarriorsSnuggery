using System.Collections.Generic;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Objects.Particles
{
	[Desc("Spawns all particles in a line.")]
	public class LineParticleSpawner : ParticleSpawner
	{
		[Desc("Type of particle to spawn.")]
		public readonly ParticleType Type;

		[Desc("Particle count.")]
		public readonly int Count = 1;

		[Desc("Radius for spawn line.")]
		public readonly int Radius = 0;

		[Desc("Angle of line in degrees.")]
		public readonly int Angle = 0;

		[Desc("Offset of the spawning point.")]
		public readonly CPos Offset = CPos.Zero;

		[Desc("Initial velocity of the particles.")]
		public readonly CPos InitialVelocity = CPos.Zero;

		public LineParticleSpawner(List<TextNode> nodes) : base(nodes) { }

		public override Particle[] Create(World world, CPos position)
		{
			var particles = new Particle[Count];
			var length = Radius / (float)Count;
			for (int i = 0; i < Count; i++)
			{
				var angle = WarriorsSnuggery.Angle.ToArc(Angle);
				var pos = CPos.FromFlatAngle(angle, length * i);

				particles[i] = ParticleCache.Create(world, Type, position + pos + Offset, InitialVelocity);
			}
			return particles;
		}

	}
}
