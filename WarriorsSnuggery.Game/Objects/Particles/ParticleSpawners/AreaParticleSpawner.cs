using System;
using System.Collections.Generic;
using WarriorsSnuggery.Loader;

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

		[Desc("Type of spawning area.")]
		public readonly ParticleAreaSpawnType AreaType = ParticleAreaSpawnType.RANDOM;

		public AreaParticleSpawner(List<TextNode> nodes) : base(nodes) { }

		public override Particle[] Create(World world, CPos position, int height)
		{
			return AreaType switch
			{
				ParticleAreaSpawnType.CIRCLE => createCircle(world, position, height),
				ParticleAreaSpawnType.BOX => createBox(world, position, height),
				_ => createRandom(world, position, height),
			};
		}

		Particle[] createRandom(World world, CPos position, int height)
		{
			var random = Program.SharedRandom;

			var particles = new Particle[Count];
			for (int i = 0; i < Count; i++)
			{
				var ran = random.Next(-Radius, Radius);
				var angle = random.NextDouble() * Math.PI * 2;
				var x = Math.Sin(angle) * ran;
				var y = Math.Cos(angle) * ran;
				var pos = new CPos((int)x, (int)y, 0);

				particles[i] = ParticleCreator.Create(world, Type, position + pos, height);
			}
			return particles;
		}

		Particle[] createCircle(World world, CPos position, int height)
		{
			var particles = new Particle[Count];
			var step = (float)(Math.PI * 2) / Count;
			for (int i = 0; i < Count; i++)
			{
				var x = Math.Sin(step * i) * Radius;
				var y = Math.Cos(step * i) * Radius;
				var pos = new CPos((int)x, (int)y, 0);

				particles[i] = ParticleCreator.Create(world, Type, position + pos, height);
			}
			return particles;
		}

		Particle[] createBox(World world, CPos position, int height)
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

				particles[i] = ParticleCreator.Create(world, Type, position + pos, height);
			}
			return particles;
		}
	}
}
