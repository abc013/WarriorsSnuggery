using System;
using System.Collections.Generic;
using WarriorsSnuggery;
using WarriorsSnuggery.Objects.Particles;
using WarriorsSnuggery.Scripting;

namespace WarriorsSnuggeryScripts
{
	public class MainMenuScript : MissionScriptBase
	{
		static readonly string[] particleTypes = { "blood", "colored_star", "fire", "grey", "firefly", "nebula1", "leaf", "small_star" };

		readonly List<ParticleEntity> entities = new List<ParticleEntity>();

		public MainMenuScript(string file, Game game) : base(file, game)
		{
			const int count = 3;
			static ParticleType particleType() => ParticleCache.Types[particleTypes[Program.SharedRandom.Next(particleTypes.Length)]];

			var random = Program.SharedRandom;
			var bounds = world.Map.Bounds.ToCPos();
			var clamp = 3072;

			for (int i = 0; i < count; i++)
				entities.Add(new ParticleEntity(world, Program.SharedRandom, particleType(), randomPosition(random, bounds, clamp), (float)random.NextDouble(), 0.004f * random.Next(1, 20), random.Next(256, 512), random.Next(5, 20)));
		}

		static CPos randomPosition(Random random, CPos bounds, int clamp)
		{
			var x = random.Next(clamp, bounds.X - clamp);
			var y = random.Next(clamp, bounds.Y - clamp);

			return new CPos(x, y, 0);
		}

		public override void Tick()
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
			int radius;

			public ParticleEntity(World world, Random random, ParticleType type, CPos position, float angle, float angleVelocity, int radius, int radiusVelocity)
			{
				this.world = world;
				this.random = random;
				this.type = type;

				this.position = position;

				negate = random.NextDouble() > 0.5f;

				this.angle = angle;
				this.angleVelocity = angleVelocity;

				this.radius = radius;
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

				radius += (negate ? -1 : 1) * (random.Next(radiusVelocity) - radiusVelocity / 2);
				position += withAngle((position - world.LocalPlayer.Position).FlatAngle + MathF.PI / 2, (negate ? -1 : 1) * radiusVelocity);
			}

			static CPos withAngle(float angle, int number)
			{
				return new CPos((int)(MathF.Cos(angle) * number), (int)(MathF.Sin(angle) * number), 0);
			}
		}
	}
}
