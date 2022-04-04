using System;
using System.Collections.Generic;
using WarriorsSnuggery;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Objects.Particles;
using WarriorsSnuggery.Scripting;

namespace WarriorsSnuggeryScripts
{
	public class MainMenuScript : MissionScriptBase
	{
		static readonly string[] particleTypes = { "blood", "colored_star", "fire", "grey", "firefly", "nebula1", "leaf", "small_star" };

		readonly List<ParticleEntity> entities = new List<ParticleEntity>();

		public MainMenuScript(PackageFile packageFile, Game game) : base(packageFile, game)
		{
			const int count = 3;
			static ParticleType particleType() => ParticleCache.Types[particleTypes[Program.SharedRandom.Next(particleTypes.Length)]];
			static CPos randomPosition(Random random, CPos bounds, int clamp) => new CPos(random.Next(clamp, bounds.X - clamp), random.Next(clamp, bounds.Y - clamp), 0);

			var random = Program.SharedRandom;
			var bounds = world.Map.Bounds.ToCPos();
			var clamp = 3072;

			for (int i = 0; i < count; i++)
				entities.Add(new ParticleEntity(world, Program.SharedRandom, particleType(), randomPosition(random, bounds, clamp), (float)random.NextDouble(), 0.004f * random.Next(1, 20), random.Next(5, 20)));

			Tick += tick;
		}


		void tick()
		{
			foreach (var entity in entities)
				entity.Tick();
		}

		class ParticleEntity
		{
			readonly World world;
			readonly Random random;
			readonly ParticleType type;

			CPos position;

			readonly bool negate;

			readonly float angleVelocity;
			float angle;
			readonly int radiusVelocity;

			public ParticleEntity(World world, Random random, ParticleType type, CPos position, float angle, float angleVelocity, int radiusVelocity)
			{
				this.world = world;
				this.random = random;
				this.type = type;

				this.position = position;

				negate = random.NextDouble() > 0.5f;

				this.angle = angle;
				this.angleVelocity = angleVelocity;

				this.radiusVelocity = radiusVelocity;
			}

			public void Tick()
			{
				angle += angleVelocity;
				var anglePos = withAngle(angle, 256);
				var x = position.X + anglePos.X + random.Next(-64, 64);
				var y = position.Y + anglePos.Y + random.Next(-64, 64);

				var pos = new CPos(x, y, 0);
				var init = new ParticleInit(type, pos, 512);
				world.Add(new Particle(world, init));

				position += withAngle((position - world.LocalPlayer.Position).FlatAngle + MathF.PI / 2, (negate ? -1 : 1) * radiusVelocity);
			}

			static CPos withAngle(float angle, int number)
			{
				return new CPos((int)(MathF.Cos(angle) * number), (int)(MathF.Sin(angle) * number), 0);
			}
		}
	}
}
