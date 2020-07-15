using System;
using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Objects.Particles;

namespace WarriorsSnuggery
{
	public sealed class ParticleLayer
	{
		public const int SectorSize = 4;
		public readonly List<Particle> Particles = new List<Particle>();

		readonly List<Particle> particlesToRemove = new List<Particle>();
		readonly List<Particle> particlesToAdd = new List<Particle>();

		readonly ParticleSector[,] sectors;
		readonly MPos bounds;

		bool firstTick = true;

		public ParticleLayer(MPos bounds)
		{
			this.bounds = new MPos((int)Math.Ceiling(bounds.X / (float)SectorSize), (int)Math.Ceiling(bounds.Y / (float)SectorSize));
			
			sectors = new ParticleSector[this.bounds.X, this.bounds.Y];
			for (var x = 0; x < this.bounds.X; x++)
				for (var y = 0; y < this.bounds.Y; y++)
					sectors[x, y] = new ParticleSector(new MPos(x, y));
		}

		public void Add(Particle particle)
		{
			particle.CheckVisibility();
			particlesToAdd.Add(particle);
		}

		public void Add(Particle[] particles)
		{
			foreach (var particle in particles)
				particle.CheckVisibility();
			particlesToAdd.AddRange(particles);
		}

		public void Update(Particle particle, bool first = false)
		{
			var oldSector = particle.Sector;
			var newSector = getSector(particle);

			if (oldSector != newSector)
			{
				if (!first)
					oldSector.Leave(particle);
				newSector.Enter(particle);
			}

			particle.Sector = newSector;
		}

		ParticleSector getSector(Particle particle)
		{
			var position = particle.Position - Map.Offset;
			var x = (int)Math.Floor(position.X / 4096f);
			var y = (int)Math.Floor(position.Y / 4096f);

			return sectors[x, y];
		}

		public void Remove(Particle particle)
		{
			particlesToRemove.Add(particle);
		}

		public void Tick()
		{
			if (particlesToAdd.Any())
			{
				foreach (var particle in particlesToAdd)
				{
					Particles.Add(particle);
					Update(particle, true);
				}
				particlesToAdd.Clear();
			}

			if (!firstTick)
			{
				foreach (var particle in Particles)
					particle.Tick();
			}
			firstTick = false;

			if (particlesToRemove.Any())
			{
				foreach (var particle in particlesToRemove)
				{
					Particles.Remove(particle);
					particle.Sector.Leave(particle);
				}
				particlesToRemove.Clear();
			}
		}

		public void Clear()
		{
			foreach (var particle in Particles)
				particle.Dispose();
			Particles.Clear();
		}
	}

	public class ParticleSector
	{
		public readonly MPos Position;

		readonly List<Particle> objects = new List<Particle>();

		public ParticleSector(MPos position)
		{
			Position = position;
		}

		public void Enter(Particle obj)
		{
			objects.Add(obj);
		}

		public void Leave(Particle obj)
		{
			objects.Remove(obj);
		}
	}
}
