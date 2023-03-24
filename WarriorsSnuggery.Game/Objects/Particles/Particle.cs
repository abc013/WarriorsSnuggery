using System;
using System.Collections.Generic;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Maps.Layers;

namespace WarriorsSnuggery.Objects.Particles
{
	public sealed class Particle : PositionableObject
	{
		[Save]
		public readonly ParticleType Type;
		[Save]
		public override CPos Position { get => base.Position; set => base.Position = value; }
		readonly World world;
		readonly Color cachedColor;

		public ParticleSector Sector;

		[Save("Duration")]
		int current;
		[Save("DissolveDuration")]
		int dissolve;

		// Z is height
		[Save("Velocity")]
		CPos velocity;
		Vector velocity_left;

		[Save("RotationVelocity")]
		VAngle rotate_velocity;

		public Particle(World world, ParticleInit init)
		{
			this.world = world;
			Type = init.Type;
			Renderable = init.Type.GetRenderable();
			Position = init.Position;

			cachedColor = Type.Color + ParticleUtils.Variety(Type.ColorVariety);

			current = init.Convert("Duration", Type.Duration);
			dissolve = init.Convert("DissolveDuration", Type.DissolveDuration);
			velocity = init.Convert("Velocity", ParticleUtils.Variety(Type.RandomVelocity) + init.InitialVelocity);
			rotate_velocity = init.Convert("RotationVelocity", ParticleUtils.AngleVariety(Type.RandomRotation));
		}

		public void AffectVelocity(ParticleForce force, float ratio, CPos origin)
		{
			var random = ParticleUtils.Random;

			var useZ = force.UseHeight && Position.Z != origin.Z;
			var zinvert = Position.Z > origin.Z;

			var angle = (Position - origin).FlatAngle;
			var xFloat = 0f;
			var yFloat = 0f;
			var zFloat = 0f;

			switch (force.Type)
			{
				case ParticleForceType.FORCE:
					xFloat = (force.Strength * MathF.Cos(angle)) * ratio;
					yFloat = (force.Strength * MathF.Sin(angle)) * ratio;
					if (useZ)
						zFloat = (zinvert ? -1 : 1) * force.Strength * ratio;
					break;
				case ParticleForceType.TURBULENCE:
					angle = Angle.RandomAngle(random);
					xFloat = (force.Strength * MathF.Cos(angle)) * ratio;
					yFloat = (force.Strength * MathF.Sin(angle)) * ratio;
					if (useZ)
						zFloat = (zinvert ? -1 : 1) * ((float)random.NextDouble() - 0.5f) * force.Strength * ratio;
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
						zFloat = force.Strength * ratio * velocity.Z / 256;
						if (Math.Abs(zFloat) > Math.Abs(velocity.Z))
							zFloat = -velocity.Z * ratio;
					}
					break;
				case ParticleForceType.VORTEX:
					angle -= MathF.PI / 2; // Rotate 90�
					xFloat = (force.Strength * MathF.Cos(angle)) * ratio;
					yFloat = (force.Strength * MathF.Sin(angle)) * ratio;
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

			var angle = Angle.Cast((origin - Position).FlatAngle - Rotation.CastToAngleRange().Z);

			var zFloat = 0f;

			switch (force.Type)
			{
				case ParticleForceType.FORCE:
					zFloat = Math.Sign(-angle) * force.Strength * ratio * 0.1f;
					zFloat = Math.Min(0.628f, zFloat);
					break;
				case ParticleForceType.TURBULENCE:
					angle = Angle.RandomAngle(random);
					zFloat = Math.Sign(-angle) * force.Strength * ratio * 0.1f;
					zFloat = Math.Min(0.628f, zFloat);
					break;
				case ParticleForceType.DRAG:
					zFloat = -force.Strength * ratio * rotate_velocity.Z * 0.1f;
					if (Math.Abs(zFloat) > Math.Abs(rotate_velocity.Z))
						zFloat = -rotate_velocity.Z * ratio;
					break;
				case ParticleForceType.VORTEX:
					zFloat = Math.Sign(-angle - MathF.PI / 2) * force.Strength * 0.1f;
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

			Position += velocity;

			if (velocity != CPos.Zero)
				world.ParticleLayer.Update(this);

			if (Type.StickToGround && OnGround)
			{
				velocity = CPos.Zero;
				rotate_velocity = VAngle.Zero;
			}

			if (current-- <= 0)
			{
				if (dissolve-- <= 0)
					Dispose();
			}
		}

		public override void Render()
		{
			if (Type.ShowShadow)
				RenderShadow();

			if (Type.IsLight)
			{
				MasterRenderer.SetRenderer(Renderer.LIGHTS);
				base.Render();
				MasterRenderer.SetRenderer(Renderer.DEFAULT);
				return;
			}

			if (Type.IsLight)
				Color = (dissolve / (float)Type.DissolveDuration) * cachedColor;
			else
				Color = cachedColor.WithAlpha(cachedColor.A * dissolve / Type.DissolveDuration);

			if (Type.DissolveScaling)
				Renderable.SetScale((dissolve / (float)Type.DissolveDuration));

			base.Render();
		}

		public override void Dispose()
		{
			base.Dispose();

			world.ParticleLayer.Remove(this);
		}

		public List<string> Save()
		{
			return SaveAttribute.GetFields(this);
		}
	}
}
