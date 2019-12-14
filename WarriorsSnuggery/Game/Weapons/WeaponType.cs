using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects.Particles;

namespace WarriorsSnuggery.Objects.Weapons
{
	public enum FalloffType
	{
		QUADRATIC,
		CUBIC,
		EXPONENTIAL,
		LINEAR,
		ROOT
	}

	public enum WeaponFireType
	{
		BULLET,
		BEAM,
		DIRECTEDBEAM
	}

	public class WeaponType
	{
		[Desc("Texture of the Smudge that will be left behind from impact.")]
		public readonly TextureInfo Smudge;

		[Desc("Highest damage value possible.")]
		public readonly int Damage;
		[Desc("Highest damage value for walls.")]
		public readonly int WallDamage;

		[Desc("Time until the weapon has been reloaded.", "Will also be used for the duration of beam weapons.")]
		public readonly int Reload;

		[Desc("Particles that will be emitted when the weapon impacts.")]
		public readonly ParticleSpawner ParticlesOnImpact;

		[Desc("Maximal Range the weapon can travel.")]
		public readonly int MaxRange = 8192;

		[Desc("Falloff of the impact.", "Possible: QUADRATIC, CUBIC, EXPONENTIAL, LINEAR, ROOT")]
		public readonly FalloffType Falloff = FalloffType.QUADRATIC;

		[Desc("Type of the Projectile.", "Possible: Bullet, Beam, InstantHit;")]
		public readonly ProjectileType Projectile;

		public WeaponType(string name, MiniTextNode[] nodes)
		{
			Loader.PartLoader.SetValues(this, nodes);

			if (Smudge != null)
				SpriteManager.AddTexture(Smudge);
		}
	}
}
