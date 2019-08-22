using System;

namespace WarriorsSnuggery.Objects
{
	public class Particle : PhysicsObject
	{
		int current;
		int dissolve;

		readonly ParticleType info;
		readonly float scale;

		CPos velocity;

		public Particle(CPos pos, ParticleType info) : base(pos, new SpriteRenderable(info.Texture.GetTextures(), tick: info.Texture.Tick))
		{
			Height = pos.Z;
			this.info = info;
			current = info.Tick;
			dissolve = info.DissolveTick;
			scale = (Program.SharedRandom.Next(2) - 1) * info.RanScale * info.Scale + info.Scale;
			Scale = scale;

			float ranVelocity() => ((float)Program.SharedRandom.NextDouble() * 2) - 1;

			velocity = new CPos((int)(ranVelocity() * info.RanVelocity.X), (int)(ranVelocity() * info.RanVelocity.Y), (int)(ranVelocity() * info.RanVelocity.Z));
		}

		public override void Tick()
		{
			base.Tick();

			Rotation += new VAngle(0, 0, info.Rotation);
			Position += velocity;
			Height += velocity.Z;
			velocity += info.Force;

			if (!info.RanScale.Equals(0f))
				Scale = scale * (float)Math.Sin(current * info.RanScale);

			if (current-- <= 0)
			{
				if (dissolve-- <= 0)
				{
					Dispose();
					return;
				}
				else
				{
					Renderable.SetColor(new Color(1f, 1f, 1f, ((float)dissolve) / info.DissolveTick));
				}
			}
		}
	}
}
