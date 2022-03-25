using System.Collections.Generic;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects.Particles;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Objects.Weapons.Projectiles
{
	public class BulletProjectile : IProjectile
	{
		[Require, Desc("Texture of the Weapon.")]
		public readonly TextureInfo Texture;
		[Desc("Ignore ambient lighting for the texture.")]
		public readonly bool IgnoreAmbience;

		[Desc("Particles that are emitted at the weapons travel time.")]
		public readonly ParticleSpawner TrailParticles;

		[Desc("Inaccuracy of the weapon.")]
		public readonly int Inaccuracy;

		[Desc("Weapon always points to the target.")]
		public readonly bool OrientateToTarget;

		[Desc("Force applied to the weapon during flight.")]
		public readonly CPos Force = CPos.Zero;

		[Desc("Startspeed of the warhead.")]
		public readonly int Speed = 32;

		[Desc("Maximum speed of the warhead.", "If smaller than Speed, this value will be the same as Speed.")]
		public readonly int MaxSpeed = 128;

		[Desc("Decides whether the projectile flies till the end of maximum range or only till the target position.")]
		public readonly bool FlyToTarget;

		public BulletProjectile(List<TextNode> nodes)
		{
			TypeLoader.SetValues(this, nodes);

			if (MaxSpeed < Speed)
				MaxSpeed = Speed;
		}

		public BatchSequence GetTexture()
		{
			var sequence = new BatchSequence(Texture);
			sequence.SetTextureFlags(IgnoreAmbience ? TextureFlags.IgnoreAmbience : TextureFlags.None);
			return sequence;
		}
	}
}
