using System;

namespace WarriorsSnuggery.Objects.Particles
{

	public enum ParticleAreaSpawnType
	{
		RANDOM,
		CIRCLE,
		BOX
	}

	[Desc("Spawns all particles in an defined area.")]
	public class AreaParticleSpawner : ParticleSpawner
	{
		[Desc("Type of particle to spawn.")]
		public readonly ParticleType Type;

		[Desc("Particle count.")]
		public readonly int Count = 1;

		[Desc("Radius for spawn area.")]
		public readonly int Radius = 0;

		[Desc("Type of spawning area.", "Available: RANDOM, CIRCLE, BOX")]
		public readonly ParticleAreaSpawnType AreaType = ParticleAreaSpawnType.RANDOM;

		public AreaParticleSpawner(MiniTextNode[] nodes) : base()
		{
			Loader.PartLoader.SetValues(this, nodes);
		}

		public override Particle[] Create(World world, CPos position, int height)
		{
			return AreaType switch
			{
				ParticleAreaSpawnType.CIRCLE => createCircle(world, world.Game.SharedRandom, position, height),
				ParticleAreaSpawnType.BOX => createBox(world, world.Game.SharedRandom, position, height),
				_ => createRandom(world, world.Game.SharedRandom, position, height),
			};
		}

		Particle[] createRandom(World world, Random random, CPos position, int height)
		{
			var particles = new Particle[Count];
			for (int i = 0; i < Count; i++)
			{
				var ran = Program.SharedRandom.Next(Radius * 2) - Radius;
				var angle = Program.SharedRandom.NextDouble() * Math.PI * 2;
				var x = Math.Sin(angle) * ran;
				var y = Math.Cos(angle) * ran;
				var pos = new CPos((int)x, (int)y, 0);

				particles[i] = ParticleCreator.Create(world, Type, position + pos, height, random);
			}
			return particles;
		}

		Particle[] createCircle(World world, Random random, CPos position, int height)
		{
			var particles = new Particle[Count];
			var step = (float)(Math.PI * 2) / Count;
			for (int i = 0; i < Count; i++)
			{
				var x = Math.Sin(step * i) * Radius;
				var y = Math.Cos(step * i) * Radius;
				var pos = new CPos((int)x, (int)y, 0);

				particles[i] = ParticleCreator.Create(world, Type, position + pos, height, random);
			}
			return particles;
		}

		Particle[] createBox(World world, Random random, CPos position, int height)
		{
			var particles = new Particle[Count];
			var step = (Radius * 2) / (Count / 4);
			var side = (byte)0;
			for (int i = 0; i < Count; i++)
			{
				if (i % (Count / 4) == 0)
					side++;

				var x = 0;
				var y = 0;
				switch (side)
				{
					case 1:
						x = -Radius;
						y = -Radius + (i % (Count / 4)) * step;
						break;
					case 2:
						x = Radius;
						y = -Radius + (i % (Count / 4)) * step;
						break;
					case 3:
						x = -Radius + (i % (Count / 4)) * step;
						y = -Radius;
						break;
					case 4:
						x = -Radius + (i % (Count / 4)) * step;
						y = Radius;
						break;
				}
				var pos = new CPos(x, y, 0);

				particles[i] = ParticleCreator.Create(world, Type, position + pos, height, random);
			}
			return particles;
		}
	}
}
