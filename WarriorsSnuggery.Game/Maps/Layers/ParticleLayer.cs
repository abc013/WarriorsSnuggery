using System;
using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Objects.Particles;

namespace WarriorsSnuggery.Maps.Layers
{
	public sealed class ParticleLayer : ISaveable
	{
		public const int SectorSize = 4;
		public readonly List<Particle> Particles = new List<Particle>();

		readonly List<Particle> particlesToRemove = new List<Particle>();
		readonly List<Particle> particlesToAdd = new List<Particle>();

		readonly ParticleSector[,] sectors;
		readonly MPos bounds;

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
			particlesToAdd.Add(particle);
		}

		public void Add(Particle[] particles)
		{
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
			var x = (int)Math.Floor(position.X / (float)(SectorSize * Constants.TileSize));
			var y = (int)Math.Floor(position.Y / (float)(SectorSize * Constants.TileSize));
			x = Math.Clamp(x, 0, bounds.X - 1);
			y = Math.Clamp(y, 0, bounds.Y - 1);

			return sectors[x, y];
		}

		public ParticleSector[] GetSectors(CPos position, int radius)
		{
			var topleft = position - new CPos(radius, radius, 0) - Map.Offset;
			var botright = position + new CPos(radius, radius, 0) - Map.Offset;

			return getSectors(topleft, botright);
		}

		ParticleSector[] getSectors(CPos topleft, CPos botright)
		{
			var pos1 = new MPos((int)Math.Clamp(Math.Floor(topleft.X / (float)(SectorSize * Constants.TileSize)), 0, bounds.X - 1), (int)Math.Clamp(Math.Floor(topleft.Y / (float)(SectorSize * Constants.TileSize)), 0, bounds.Y - 1));
			var pos2 = new MPos((int)Math.Clamp(Math.Ceiling(botright.X / (float)(SectorSize * Constants.TileSize)), 0, bounds.X - 1), (int)Math.Clamp(Math.Ceiling(botright.Y / (float)(SectorSize * Constants.TileSize)), 0, bounds.Y - 1));

			var sectors = new ParticleSector[(pos2.X - pos1.X + 1) * (pos2.Y - pos1.Y + 1)];
			var i = 0;
			for (var x = pos1.X; x < pos2.X + 1; x++)
				for (var y = pos1.Y; y < pos2.Y + 1; y++)
					sectors[i++] = this.sectors[x, y];

			return sectors;
		}

		public void Remove(Particle particle)
		{
			particlesToRemove.Add(particle);
		}

		public void Tick()
		{
			if (particlesToAdd.Count != 0)
			{
				foreach (var particle in particlesToAdd)
				{
					Particles.Add(particle);
					Update(particle, true);
				}
				particlesToAdd.Clear();
			}

			foreach (var particle in Particles)
				particle.Tick();

			if (particlesToRemove.Count != 0)
			{
				foreach (var particle in particlesToRemove)
				{
					Particles.Remove(particle);

					particle.Sector.Leave(particle);
				}
				particlesToRemove.Clear();
			}
		}

		public HashSet<Particle> GetVisible(CPos topleft, CPos bottomright)
		{
			var visibleParticles = new List<Particle>();
			var sectors = getSectors(topleft, bottomright);

			foreach (var sector in sectors)
				visibleParticles.AddRange(sector.Particles);

			return visibleParticles.ToHashSet();
		}

		public TextNodeSaver Save()
		{
			var saver = new TextNodeSaver();
			for (int i = 0; i < Particles.Count; i++)
				saver.AddChildren($"{i}", Particles[i].Save());

			return saver;
		}
	}

	public class ParticleSector
	{
		public readonly MPos Position;

		public readonly List<Particle> Particles = new List<Particle>();

		public ParticleSector(MPos position)
		{
			Position = position;
		}

		public void Enter(Particle particle)
		{
			Particles.Add(particle);
		}

		public void Leave(Particle particle)
		{
			Particles.Remove(particle);
		}
	}
}
