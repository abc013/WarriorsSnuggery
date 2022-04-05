using System.Collections.Generic;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Objects.Particles
{
	public class ParticleType
	{
		[Desc("Texture to use for the particle.", "The game will crash when both Texture and MeshSize are not defined or zero.")]
		public readonly TextureInfo Texture;
		[Desc("Color to use for particle.")]
		public readonly Color Color = Color.White;
		[Desc("Random color modification.")]
		public readonly Color ColorVariety;
		[Desc("Size of the particle when using monocolored particles in pixels.", "The game will crash when both Texture and MeshSize are not defined or zero.", "Does not work if Texture is defined.")]
		public readonly int MeshSize;
		[Desc("Random scale when using monocolored particles.")]
		public readonly float MeshSizeVariety;
		[Desc("Determines whether to render the particle as light.")]
		public readonly bool IsLight;
		[Desc("Determines whether to render the particle without using ambience.", "This can be used to create 'glow in the dark' effects.")]
		public readonly bool IgnoreAmbience;

		[Desc("Gravitational force.")]
		public readonly int Gravity = 2;

		[Desc("Random velocity given at start.")]
		public readonly CPos RandomVelocity = CPos.Zero;
		[Desc("Random rotational velocity given at start.")]
		public readonly CPos RandomRotation = CPos.Zero;
		[Desc("Random scale added to one, given at start.")]
		public readonly float RandomScale = 0f;

		[Desc("Time in which the particle is alive.")]
		public readonly int Duration;
		[Desc("Time in which the particle is dissolving.", "This time will be added on the time given in Duration.")]
		public readonly int DissolveDuration;
		[Desc("Determines whether particles should also scale down during dissolve.")]
		public readonly bool DissolveScaling = true;

		[Desc("Show shadow of the particle.")]
		public readonly bool ShowShadow = false;
		[Desc("Stops any rotation or position changes while on ground.")]
		public readonly bool StickToGround = false;
		[Desc("Regulates whether force can be applied on the particle from other objects.")]
		public readonly bool AffectedByObjects = false;

		public ParticleType(List<TextNode> nodes)
		{
			TypeLoader.SetValues(this, nodes);
		}

		public BatchRenderable GetRenderable()
		{
			var color = Color + ParticleUtils.Variety(ColorVariety);

			BatchRenderable renderable;
			if (Texture == null)
				renderable = new BatchObject(MeshSize * Constants.PixelMultiplier + ParticleUtils.Variety(MeshSizeVariety));
			else
				renderable = new BatchSequence(Texture);

			renderable.SetColor(color);
			renderable.SetTextureFlags(IgnoreAmbience ? TextureFlags.IgnoreAmbience : TextureFlags.None);
			return renderable;
		}

		public override string ToString()
		{
			return ParticleCache.Types[this];
		}
	}
}
