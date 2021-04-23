using System.Collections.Generic;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects.Particles;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Objects.Weapons.Projectiles
{
	public class BeamProjectile : IProjectile
	{
		[Desc("Texture of the Beam.")]
		public readonly TextureInfo Beam;
		[Desc("Texture of the Beam.", "This texture will show up when launching the weapon.")]
		public readonly TextureInfo BeamStartUp;
		[Desc("Texture of the Beam.", "This texture will show up as cooldown animation.")]
		public readonly TextureInfo BeamCooldown;

		[Desc("Particles that are emitted along the path of the beamweapon.")]
		public readonly ParticleSpawner BeamParticles;
		[Desc("Distance between the spawners along the path of the beamweapon.")]
		public readonly int BeamParticleDistance = 128;
		[Desc("Time between particle spawns along the path of the beamweapon.")]
		public readonly int BeamParticleTick = 2;

		[Desc("Sound to play while the beam is running. This sound is looping.")]
		public readonly SoundType BeamSound;

		[Desc("Inaccuracy of the weapon.")]
		public readonly int Inaccuracy;

		[Desc("Interval in which the beam will detonate its warhead.", "Works only if the WeaponFireType is BEAM or DIRECTEDBEAM.")]
		public readonly int ImpactInterval;

		[Desc("Determines how the beam needs to be fired up.")]
		public readonly int StartupDuration;
		[Desc("Determines how long the beam stays after ending.")]
		public readonly int CooldownDuration;

		[Desc("Determines whether the beam target can be moved.")]
		public readonly bool Directed;
		[Desc("Determines the turn speed of the beam in degrees.")]
		public readonly int TurnSpeed = 5;

		public readonly float ArcTurnSpeed;

		public BeamProjectile(List<TextNode> nodes)
		{
			TypeLoader.SetValues(this, nodes);

			ArcTurnSpeed = Angle.ToArc(TurnSpeed) / 10f;
		}

		public BatchSequence GetTexture()
		{
			return null;
		}
	}
}
