using System.Collections.Generic;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Objects.Particles;

namespace WarriorsSnuggery.Objects.Weapons.Projectiles
{
	public class SplashProjectile : IProjectile
	{
		[Require, Desc("Texture of the Weapon.")]
		public readonly TextureInfo Texture;
		[Desc("Ignore ambient lighting for the texture.")]
		public readonly bool IgnoreAmbience;

		[Desc("Particles that are emitted at the weapons travel time.")]
		public readonly ParticleSpawner TrailParticles;

		[Desc("Inaccuracy of the weapon.")]
		public readonly int Inaccuracy;

		[Desc("Don't despawn if collision with another object occurs.")]
		public readonly bool IgnoreCollisions;

		[Desc("Sets the distance in such a way that all explosions occur between origin and target.")]
		public readonly bool DistanceBasedOnTarget;

		[Desc("Maximal number of explosions that are triggered.")]
		public readonly int Repetitions;

		[Desc("Delay between single explosions.")]
		public readonly int RepetitionDelay = 32;

		[Desc("Distance between explosions.", $"Can be overriden by {nameof(DistanceBasedOnTarget)}.")]
		public readonly int RepetitionDistance = 512;

		public int DistancePerTick => RepetitionDistance / RepetitionDelay;

		public SplashProjectile(List<TextNode> nodes)
		{
			TypeLoader.SetValues(this, nodes);
		}

		public BatchSequence GetTexture()
		{
			var sequence = new BatchSequence(Texture);
			sequence.SetTextureFlags(IgnoreAmbience ? TextureFlags.IgnoreAmbience : TextureFlags.None);
			return sequence;
		}
	}
}
