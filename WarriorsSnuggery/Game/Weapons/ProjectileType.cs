using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects.Particles;

namespace WarriorsSnuggery.Objects.Weapons
{
	public abstract class ProjectileType
	{
	}

	public class InstantHitProjectileType : ProjectileType
	{
		[Desc("Chance of the weapon to hit.")]
		public readonly float HitChance;

		public InstantHitProjectileType(MiniTextNode[] nodes)
		{
			Loader.PartLoader.SetValues(this, nodes);
		}
	}

	public class BulletProjectileType : ProjectileType
	{
		[Desc("Texture of the Weapon.")]
		public readonly TextureInfo Texture;

		[Desc("Speed of the warhead.")]
		public readonly int Speed;

		[Desc("Particles that are emitted at the weapons travel time.")]
		public readonly ParticleSpawner TrailParticles;

		[Desc("Inaccuracy of the weapon.")]
		public readonly int Inaccuracy;

		[Desc("Weapon always points to the target.")]
		public readonly bool OrientateToTarget;

		[Desc("Angle at which the weapon should be launched at minimum.", "Minimum is -89, Maximum is 89.")]
		public readonly int MinimumAngle = 0;
		[Desc("Angle at which the weapon should be launched at maximum.", "Minimum is -89, Maximum is 89.")]
		public readonly int MaximumAngle = 89;

		[Desc("Gravity applied to the weapon.")]
		public readonly int Gravity = 0;

		public BulletProjectileType(MiniTextNode[] nodes)
		{
			Loader.PartLoader.SetValues(this, nodes);

			if (Texture != null)
				SpriteManager.AddTexture(Texture);
		}
	}

	public class BeamProjectileType : ProjectileType
	{
		[Desc("Texture of the Beam.")]
		public readonly TextureInfo Beam;
		[Desc("Texture of the Beam.", "This texture will show up when launching the weapob.")]
		public readonly TextureInfo BeamStartUp;
		[Desc("Texture of the Beam.", "This texture will show up as cooldown animation.")]
		public readonly TextureInfo BeamCooldown;

		[Desc("Particles that are emitted along the path of the beamweapon.")]
		public readonly ParticleSpawner BeamParticles;

		[Desc("Inaccuracy of the weapon.")]
		public readonly int Inaccuracy;

		[Desc("Interval in which the beam will detonate its warhead.", "Works only if the WeaponFireType is BEAM or DIRECTEDBEAM.")]
		public readonly int ImpactInterval;

		[Desc("Determines how long a beam will be fired.")]
		public readonly int BeamDuration;
		[Desc("Determines how the beam needs to be fired up.")]
		public readonly int StartupDuration;
		[Desc("Determines how long the beam stays after ending.")]
		public readonly int CooldownDuration;

		[Desc("Determines whether the beam target can be moved.")]
		public readonly bool Directed;

		public BeamProjectileType(MiniTextNode[] nodes)
		{
			Loader.PartLoader.SetValues(this, nodes);

			if (Beam != null)
				SpriteManager.AddTexture(Beam);
			if (BeamStartUp != null)
				SpriteManager.AddTexture(BeamStartUp);
			if (BeamCooldown != null)
				SpriteManager.AddTexture(BeamCooldown);
		}
	}
}
