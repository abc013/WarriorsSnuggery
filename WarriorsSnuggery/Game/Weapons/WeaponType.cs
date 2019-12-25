﻿using WarriorsSnuggery.Objects.Particles;

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
		[Desc("Time until the weapon has been reloaded.")]
		public readonly int Reload;

		[Desc("Time to wait until the weapon is fired.")]
		public readonly int PreparationDelay;

		[Desc("Time to wait after the weapon is fired.")]
		public readonly int CooldownDelay;

		[Desc("Maximal Range the weapon can travel.")]
		public readonly int MaxRange = 8192;

		[Desc("Type of the Projectile.")]
		public readonly IProjectileType Projectile;

		[Desc("Contains all different kinds of warheads that will impact when the weapon hits the target.")]
		public readonly IWarhead[] Warheads;

		public WeaponType(MiniTextNode[] nodes)
		{
			Loader.PartLoader.SetValues(this, nodes);
		}
	}
}
