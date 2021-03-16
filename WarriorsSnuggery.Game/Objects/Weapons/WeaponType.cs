using System.Collections.Generic;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Objects.Weapons.Projectiles;
using WarriorsSnuggery.Objects.Weapons.Warheads;

namespace WarriorsSnuggery.Objects.Weapons
{
	public class WeaponType
	{
		[Desc("Time until the weapon has been reloaded.")]
		public readonly int Reload;

		[Desc("Time to wait until the weapon is fired.", "This parameter has to be set in order to use the StartsAttacking condition.")]
		public readonly int PreparationDelay;

		[Desc("Time it takes to shoot the weapon.", "This parameter has to be set in order to use the IsAttacking condition.")]
		public readonly int ShootDuration;

		[Desc("Time to wait after the weapon is fired.", "This parameter has to be set in order to use the EndsAttacking condition.")]
		public readonly int CooldownDelay;

		[Desc("Maximal Range the weapon can travel.")]
		public readonly int MaxRange = 8192;

		[Desc("Sound to be played on firing.")]
		public readonly SoundType FireSound;

		[Desc("Type of the Projectile.")]
		public readonly IProjectile Projectile;

		[Desc("Contains all different kinds of warheads that will impact when the weapon hits the target.")]
		public readonly IWarhead[] Warheads;

		public WeaponType(List<TextNode> nodes)
		{
			TypeLoader.SetValues(this, nodes);
		}

		public override string ToString()
		{
			return WeaponCreator.GetName(this);
		}
	}
}
