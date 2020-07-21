using System;
using System.Collections.Generic;
using System.Linq;

namespace WarriorsSnuggery.Objects.Particles
{
	public class Particle : PositionableObject
	{
		public readonly bool AffectedByObjects;
		public readonly ParticleType Type;
		readonly World world;

		public ParticleSector Sector;

		int current;
		int dissolve;

		// Z is height
		CPos velocity;
		Vector velocity_left;

		VAngle rotate_velocity;

		public Particle(World world, CPos pos, int height, ParticleType type) : base(pos, type.GetRenderable())
		{
			Height = height;
			Type = type;
			this.world = world;

			AffectedByObjects = type.AffectedByObjects;
			current = type.Duration;
			dissolve = type.DissolveDuration;

			velocity = ParticleUtils.Variety(type.RandomVelocity);
			rotate_velocity = ParticleUtils.AngleVariety(type.RandomRotation);
		}

		public Particle(World world, ParticleInit init) : base(init.Position, init.Type.GetRenderable())
		{
			this.world = world;
			Type = init.Type;

			Height = init.Height;
			AffectedByObjects = Type.AffectedByObjects;

			current = init.Convert("Duration", Type.Duration);
			dissolve = init.Convert("DissolveDuration", Type.DissolveDuration);

			velocity = init.Convert("Velocity", CPos.Zero);
			rotate_velocity = init.Convert("RotationVelocity", VAngle.Zero);
		}

		public void AffectVelocity(ParticleForce force, float ratio, CPos origin)
		{
			var random = ParticleUtils.Random;

			var useZ = force.UseHeight;

			var angle = (Position - origin).FlatAngle;
			var xFloat = 0f;
			var yFloat = 0f;
			var zFloat = 0f;

			switch (force.Type)
			{
				case ParticleForceType.FORCE:
					xFloat = (float)(force.Strength * Math.Cos(angle)) * ratio;
					yFloat = (float)(force.Strength * Math.Sin(angle)) * ratio;
					if (useZ)
						zFloat = force.Strength * ratio;
					break;
				case ParticleForceType.TURBULENCE:
					angle = (float)(random.NextDouble() * 2 * Math.PI);
					xFloat = (float)(force.Strength * Math.Cos(angle)) * ratio;
					yFloat = (float)(force.Strength * Math.Sin(angle)) * ratio;
					if (useZ)
						zFloat = ((float)random.NextDouble() - 0.5f) * force.Strength * ratio;
					break;
				case ParticleForceType.DRAG:
					xFloat = -force.Strength * ratio * velocity.X / 256;
					if (Math.Abs(xFloat) > Math.Abs(velocity.X))
						xFloat = -velocity.X * ratio;

					yFloat = -force.Strength * ratio * velocity.Y / 256;
					if (Math.Abs(yFloat) > Math.Abs(velocity.Y))
						yFloat = -velocity.Y * ratio;

					if (useZ)
					{
						zFloat = -force.Strength * ratio * velocity.Z / 256;
						if (Math.Abs(zFloat) > Math.Abs(velocity.Z))
							zFloat = -velocity.Z * ratio;
					}
					break;
				case ParticleForceType.VORTEX:
					angle -= (float)Math.PI / 2;
					xFloat = (float)(force.Strength * Math.Cos(angle)) * ratio;
					yFloat = (float)(force.Strength * Math.Sin(angle)) * ratio;
					zFloat = 0; // Vortex is only 2D
					break;
			}

			xFloat += velocity_left.X;
			yFloat += velocity_left.Y;
			zFloat += velocity_left.Z;

			var x = (int)Math.Round(xFloat);
			var y = (int)Math.Round(yFloat);
			var z = (int)Math.Round(zFloat);

			velocity_left = new Vector(xFloat - x, yFloat - y, zFloat - z);
			velocity += new CPos(x, y, z);
		}

		public void AffectRotation(ParticleForce force, float ratio, CPos origin)
		{
			var random = ParticleUtils.Random;

			var angle = (origin - Position).FlatAngle - 2 * (float)Math.PI + Rotation.CastToAngleRange().Z;

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

			velocity += new CPos(0, 0, -Type.Gravity);
			Rotation += rotate_velocity;

			Position += new CPos(velocity.X, velocity.Y, 0);

			if (velocity != CPos.Zero)
				world.ParticleLayer.Update(this);

			Height += velocity.Z;
			if (Height < 0)
				Height = 0;

			if (current-- <= 0)
			{
				if (dissolve-- <= 0)
					Dispose();
				else
					Renderable.SetColor(new Color(Type.Color.R, Type.Color.G, Type.Color.B, Type.Color.A * dissolve / Type.DissolveDuration));
			}
		}

		public override void Render()
		{
			if (Type.ShowShadow)
				RenderShadow();

			base.Render();
		}

		public override void Dispose()
		{
			base.Dispose();

			world.ParticleLayer.Remove(this);
		}

		public List<string> Save()
		{
			var list = new List<string>
			{
				"Position=" + Position,
				"Height=" + Height,
				"Type=" + ParticleCreator.Types.FirstOrDefault(t => t.Value == Type).Key,
				"Duration=" + current,
				"DissolveDuration=" + dissolve,
				"Velocity=" + velocity,
				"RotationVelocity=" + rotate_velocity
			};

			return list;
		}
	}
}
