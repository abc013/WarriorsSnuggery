/*
 * User: Andreas
 * Date: 08.12.2017
 * 
 */
using System;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.Objects.Particles
{
	public class Particle : PhysicsObject
	{
		public readonly bool AffectedByObjects;
		public readonly string Name;
		readonly ParticleType type;
		readonly Random random;

		int current;
		int dissolve;

		// Z is height
		CPos transform_velocity;
		VAngle rotate_velocity;

		// TODO: make color use only one mesh or sth like that
		public Particle(CPos pos, int height, ParticleType type, Random random) : base(pos, type.Texture != null ? (GraphicsObject)new IImageSequenceRenderable(type.Texture.GetTextures(), tick: type.Texture.Tick) : new ColoredRectRenderable(Color.White, type.MeshSize / 24f, Graphics.DrawMethod.TRIANGLE))
		{
			Height = height;
			this.type = type;
			this.random = random;

			AffectedByObjects = type.AffectedByObjects;
			Name = ParticleCreator.GetName(type);
			current = type.Duration;
			dissolve = type.DissolveDuration;

			Renderable.SetColor(type.Color);

			transform_velocity = new CPos(random.Next(-type.RandomVelocity.X, type.RandomVelocity.X), random.Next(-type.RandomVelocity.Y, type.RandomVelocity.Y), random.Next(-type.RandomVelocity.Z, type.RandomVelocity.Z));
			rotate_velocity = new VAngle(random.Next(-type.RandomRotation.X, type.RandomRotation.X), random.Next(-type.RandomRotation.Y, type.RandomRotation.Y), random.Next(-type.RandomRotation.Z, type.RandomRotation.Z));
		}

		float xLeft;
		float yLeft;
		// TODO add z
		public void AffectVelocity(ParticleForce force, float ratio, CPos origin)
		{
			var angle = origin.AngleToXY(Position);
			var xFloat = 0f;
			var yFloat = 0f;

			switch(force.Type)
			{
				case ParticleForceType.FORCE:
					xFloat = (float)(force.Strength * Math.Cos(angle)) * ratio;
					yFloat = (float)(force.Strength * Math.Sin(angle)) * ratio;
					break;
				case ParticleForceType.TURBULENCE:
					angle = (float)(random.NextDouble() * 2 * Math.PI);
					xFloat = (float)(force.Strength * Math.Cos(angle)) * ratio;
					yFloat = (float)(force.Strength * Math.Sin(angle)) * ratio;
					break;
				case ParticleForceType.DRAG:
					xFloat = -force.Strength * ratio * transform_velocity.X / 256;
					if (Math.Abs(xFloat) > Math.Abs(transform_velocity.X))
						xFloat = -transform_velocity.X * ratio;

					yFloat = -force.Strength * ratio * transform_velocity.Y / 256;
					if (Math.Abs(yFloat) > Math.Abs(transform_velocity.Y))
						yFloat = -transform_velocity.Y * ratio;
					break;
				case ParticleForceType.VORTEX:
					angle -= (float)Math.PI / 2;
					xFloat = (float)(force.Strength * Math.Cos(angle)) * ratio;
					yFloat = (float)(force.Strength * Math.Sin(angle)) * ratio;
					break;
			}
			var x = (int)Math.Round(xFloat + xLeft);
			var y = (int)Math.Round(yFloat + yLeft);
			xLeft = (xFloat + xLeft) - x;
			yLeft = (yFloat + yLeft) - y;
			transform_velocity += new CPos(x, y, 0);
		}

		public void AffectRotation(ParticleForce force, float ratio, CPos origin)
		{
			var angle = Position.AngleToXY(origin) - 2*(float)Math.PI + Rotation.CastToAngleRange().Z; //TODO make better

			if (angle < -Math.PI)
				angle += 2 * (float)Math.PI;

			if (angle > Math.PI)
				angle -= 2 * (float)Math.PI;

			var zFloat = 0f;

			switch (force.Type)
			{
				case ParticleForceType.FORCE:
					zFloat = Math.Sign(-angle) * force.Strength * ratio * 0.1f;
					zFloat = Math.Min(0.628f, zFloat);
					break;
				case ParticleForceType.TURBULENCE:
					angle = (float)(random.NextDouble() * 2 * Math.PI);
					zFloat = Math.Sign(-angle) * force.Strength * ratio * 0.1f;
					zFloat = Math.Min(0.628f, zFloat);
					break;
				case ParticleForceType.DRAG:
					zFloat = -force.Strength * ratio * rotate_velocity.Z * 0.1f;
					if (Math.Abs(zFloat) > Math.Abs(rotate_velocity.Z))
						zFloat = -rotate_velocity.Z * ratio;
					break;
				case ParticleForceType.VORTEX:
					zFloat = Math.Sign(-angle - (float)Math.PI / 2) * force.Strength * 0.1f;
					zFloat = Math.Min(0.628f, zFloat);
					break;
			}

			rotate_velocity = new VAngle(0, 0, zFloat);
		}

		public override void Tick()
		{
			base.Tick();
			
			transform_velocity += new CPos(0, 0, -type.Gravity);
			Rotation += rotate_velocity;

			Position += new CPos(transform_velocity.X, transform_velocity.Y, 0);
			Height += transform_velocity.Z;
			if (Height < 0)
				Height = 0;

			if (current-- <= 0)
			{
				if (dissolve-- <= 0)
				{
					Dispose();
					return;
				}
				else
				{
					Renderable.SetColor(new Color(type.Color.R, type.Color.B, type.Color.G, type.Color.A * ((float)dissolve) / type.DissolveDuration));
				}
			}
		}

		public override void Render()
		{
			if (type.ShowShadow)
				RenderShadow();

			base.Render();
		}

		public override void Dispose()
		{
			if (type.Texture == null)
				Renderable.Dispose();

			base.Dispose();
		}
	}
}
