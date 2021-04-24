using System;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.Objects.Weather
{
	public class WeatherEffectController
	{
		readonly WeatherEffect effect;
		public bool UsesLines => effect.Type == WeatherEffectType.RAIN;

		readonly WeatherParticle[] particles;

		float windTick;

		public WeatherEffectController(World world, WeatherEffect effect)
		{
			this.effect = effect;

			var particleCount = effect.Particles * 2;

			particles = new WeatherParticle[particleCount];
			for (int i = 0; i < particleCount; i++)
				particles[i] = new WeatherParticle(world, randomPosition(), effect);
		}

		public void Tick()
		{
			windTick += effect.WindChangeSpeed * Program.SharedRandom.Next(100) / 100f;
			var wind = new CPos((int)(Math.Sin(windTick) * effect.WindVariation.X), (int)(Math.Cos(windTick) * effect.WindVariation.Y), 0);

			foreach (var particle in particles)
			{
				particle.Position += wind;
				particle.Tick();
			}
		}

		static CPos randomPosition()
		{
			var random = Program.SharedRandom;

			var x = random.Next((int)(Camera.MaxZoom * WindowInfo.Ratio * 1024)) - (int)(Camera.MaxZoom * WindowInfo.Ratio * 512);
			var y = random.Next((int)(Camera.MaxZoom * 1024)) - (int)(Camera.MaxZoom * 512);

			return new CPos(x, y, 0) + Camera.LookAt;
		}

		public void Render()
		{
			foreach (var particle in particles)
				particle.Render();
		}

		class WeatherParticle
		{
			readonly World world;

			public CPos Position;

			readonly bool isQuad;
			readonly Color color;
			readonly int size;
			readonly CPos velocity;

			public WeatherParticle(World world, CPos position, WeatherEffect effect)
			{
				this.world = world;
				Position = position;
				isQuad = effect.Type == WeatherEffectType.SNOW;

				color = effect.Color + variety(effect.ColorVariety);
				size = (int)Math.Max(0, effect.Size + variety(effect.RandomSize));
				velocity = effect.Velocity + variety(effect.RandomVelocity);
			}

			public void Tick()
			{
				Position += velocity;

				if (Position.X < WeatherManager.cameraLeft)
					Position = new CPos(Position.X + WeatherManager.cameraMaxBounds.X, Position.Y, Position.Z);
				else if (Position.X > WeatherManager.cameraRight)
					Position = new CPos(Position.X - WeatherManager.cameraMaxBounds.X, Position.Y, Position.Z);

				if (Position.Y < WeatherManager.cameraTop)
					Position = new CPos(Position.X, Position.Y + WeatherManager.cameraMaxBounds.Y, Position.Z);
				else if (Position.Y > WeatherManager.cameraBottom)
					Position = new CPos(Position.X, Position.Y - WeatherManager.cameraMaxBounds.Y, Position.Z);
			}

			public void Render()
			{
				if (!Camera.IsVisible(Position, 0, 0))
					return;

				if (!world.IsInWorld(Position))
					return;

				if (isQuad)
					ColorManager.DrawQuad(Position, size, color);
				else
					ColorManager.DrawLine(Position, Position + new CPos(velocity.X * 10, velocity.Y * 10, 0), color);
			}

			static readonly Random random = Program.SharedRandom;

			static Color variety(Color value)
			{
				return new Color(variety(value.R), variety(value.G), variety(value.B), variety(value.A));
			}

			static float variety(float value)
			{
				return ((float)random.NextDouble() - 0.5f) * value;
			}

			static CPos variety(CPos value)
			{
				return new CPos(radius(value.X), radius(value.Y), radius(value.Z));
			}

			static int radius(int radius)
			{
				return random.Next(-radius, radius);
			}
		}
	}
}
