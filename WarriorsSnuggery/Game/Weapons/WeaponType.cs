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

	public class WeaponType
	{
		[Desc("Time until the weapon has been reloaded.", "Will also be used for the duration of beam weapons.")]
		public readonly int Reload;

		[Desc("Maximal Range the weapon can travel.")]
		public readonly int MaxRange = 8192;

		[Desc("Type of the Projectile.", "Possible: Bullet, Beam, InstantHit;")]
		public readonly IProjectileType Projectile;

		[Desc("Contains all different kinds of warheads that will impact when the weapon hits the target.")]
		public readonly IWarhead[] Warheads;

		public WeaponType(MiniTextNode[] nodes)
		{
			Loader.PartLoader.SetValues(this, nodes);
		}
	}
}
