﻿using System.Collections.Generic;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Objects.Particles;

namespace WarriorsSnuggery.Objects.Weapons.Projectiles
{
	public class MagicProjectile : IProjectile
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

		[Desc("Weapon follows the target.")]
		public readonly bool FollowTarget;

		[Desc("Speed of the warhead.")]
		public readonly int Speed = 32;

		[Desc("Turnspeed of the warhead in degrees.")]
		public readonly int TurnSpeed = 32;

		public readonly float ArcTurnSpeed;

		[Desc("Turbulence to build in in degrees.")]
		public readonly int Turbulence = 0;

		public MagicProjectile(List<TextNode> nodes)
		{
			TypeLoader.SetValues(this, nodes);

			ArcTurnSpeed = Angle.ToArc(TurnSpeed);
		}

		public BatchSequence GetTexture()
		{
			var sequence = new BatchSequence(Texture);
			sequence.SetTextureFlags(IgnoreAmbience ? TextureFlags.IgnoreAmbience : TextureFlags.None);
			return sequence;
		}
	}
}
