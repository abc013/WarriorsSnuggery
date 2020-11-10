using System.Collections.Generic;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects.Particles;

namespace WarriorsSnuggery.Objects.Weapons
{
	public interface IProjectileType
	{
		BatchSequence GetTexture();
	}

	public class InstantHitProjectileType : IProjectileType
	{
		[Desc("Chance of the weapon to hit.")]
		public readonly float HitChance = 1.0f;
		[Desc("Determines whether only the target should be damaged when hit.")]
		public readonly bool Splash = true;

		public InstantHitProjectileType(List<MiniTextNode> nodes)
		{
			Loader.PartLoader.SetValues(this, nodes);
		}

		public BatchSequence GetTexture()
		{
			return null;
		}
	}

	public class BulletProjectileType : IProjectileType
	{
		[Desc("Texture of the Weapon.")]
		public readonly TextureInfo Texture;

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

		public BulletProjectileType(List<MiniTextNode> nodes)
		{
			Loader.PartLoader.SetValues(this, nodes);

			if (Texture != null)
				SpriteManager.AddTexture(Texture);

			if (MaxSpeed < Speed)
				MaxSpeed = Speed;
		}

		public BatchSequence GetTexture()
		{
			return new BatchSequence(Texture.GetTextures(), Color.White, Texture.Tick);
		}
	}

	public class MagicProjectileType : IProjectileType
	{
		[Desc("Texture of the Weapon.")]
		public readonly TextureInfo Texture;

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

		[Desc("Turnspeed of the warhead.")]
		public readonly int TurnSpeed = 32;

		public readonly float FloatTurnSpeed;

		[Desc("Turbulence to build in.")]
		public readonly int Turbulence = 0;

		public MagicProjectileType(List<MiniTextNode> nodes)
		{
			Loader.PartLoader.SetValues(this, nodes);
			FloatTurnSpeed = TurnSpeed / (float)(180 * System.Math.PI);

			if (Texture != null)
				SpriteManager.AddTexture(Texture);
		}

		public BatchSequence GetTexture()
		{
			return new BatchSequence(Texture.GetTextures(), Color.White, Texture.Tick);
		}
	}

	public class BeamProjectileType : IProjectileType
	{
		[Desc("Texture of the Beam.")]
		public readonly TextureInfo Beam;
		[Desc("Texture of the Beam.", "This texture will show up when launching the weapon.")]
		public readonly TextureInfo BeamStartUp;
		[Desc("Texture of the Beam.", "This texture will show up as cooldown animation.")]
		public readonly TextureInfo BeamCooldown;

		[Desc("Particles that are emitted along the path of the beamweapon.")]
		public readonly ParticleSpawner BeamParticles;

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
		[Desc("Determines the speed of the beam target when being moved.")]
		public readonly int MovementSpeed = 5;

		public BeamProjectileType(List<MiniTextNode> nodes)
		{
			Loader.PartLoader.SetValues(this, nodes);

			if (Beam != null)
				SpriteManager.AddTexture(Beam);
			if (BeamStartUp != null)
				SpriteManager.AddTexture(BeamStartUp);
			if (BeamCooldown != null)
				SpriteManager.AddTexture(BeamCooldown);
		}

		public BatchSequence GetTexture()
		{
			return null;
		}
	}
}
