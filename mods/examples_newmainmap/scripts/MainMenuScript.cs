using System;
using System.Collections.Generic;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Objects.Particles;
using WarriorsSnuggery.Scripting;

namespace WarriorsSnuggery.Scripts.Mods.MainmapOverride
{
	public class MainMenuScript : MissionScriptBase
	{
		readonly List<ParticleEntity> entities = new List<ParticleEntity>();

		public MainMenuScript(PackageFile packageFile, Game game) : base(packageFile, game)
		{
			Tick += tick;
		}

		void tick()
		{
			if (Program.SharedRandom.Next(100) > 98)
				addEntity();

			foreach (var entity in entities)
				entity.Tick();

			entities.RemoveAll(e => !e.Alive);
		}

		void addEntity()
		{
			static CPos randomPosition(Random random, CPos bounds, int clamp) => new CPos(random.Next(clamp, bounds.X - clamp), random.Next(clamp, bounds.Y - clamp), 0);

			var random = Program.SharedRandom;
			var bounds = world.Map.Bounds.ToCPos();
			var clamp = 4096;

			entities.Add(new ParticleEntity(world, randomPosition(random, bounds, clamp), ((float)random.NextDouble()) * Angle.MaxRange));
		}

		class ParticleEntity
		{
			readonly World world;

			readonly ParticleType smoke;
			readonly ParticleType flame;

			CPos position;
			int height;

			readonly float angle;
			const int speed = 32;

			public bool Alive => height > 0;

			public ParticleEntity(World world, CPos position, float angle)
			{
				this.world = world;
				this.position = position;
				this.angle = angle;

				height = 10240;
				smoke = ParticleCache.Types["blood"];
				flame = ParticleCache.Types["puff1"];
			}

			public void Tick()
			{
				position += CPos.FromFlatAngle(angle, speed);
				height -= speed;

				world.Add(new Particle(world, new ParticleInit(smoke, position, height)));

				world.Add(new Particle(world, new ParticleInit(flame, position, height)));
			}
		}
	}
}
