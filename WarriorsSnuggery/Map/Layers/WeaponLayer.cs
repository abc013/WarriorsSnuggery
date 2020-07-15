using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Objects.Weapons;

namespace WarriorsSnuggery
{
	public class WeaponLayer
	{
		// There are not many weapons ingame, so no sectors are needed
		public readonly List<Weapon> Weapons = new List<Weapon>();

		readonly List<Weapon> weaponsToRemove = new List<Weapon>();
		readonly List<Weapon> weaponsToAdd = new List<Weapon>();

		bool firstTick = true;

		public WeaponLayer() { }

		public void Add(Weapon weapon)
		{
			weapon.CheckVisibility();
			weaponsToAdd.Add(weapon);
		}

		public void Remove(Weapon weapon)
		{
			weaponsToRemove.Add(weapon);
		}

		public void Tick()
		{
			if (weaponsToAdd.Any())
			{
				Weapons.AddRange(weaponsToAdd);
				weaponsToAdd.Clear();
			}

			if (!firstTick)
			{
				foreach (var weapon in Weapons)
					weapon.Tick();
			}
			firstTick = false;

			if (weaponsToRemove.Any())
			{
				foreach (var weapon in weaponsToRemove)
					Weapons.Remove(weapon);
				weaponsToRemove.Clear();
			}
		}

		public void Clear()
		{
			foreach (var weapon in Weapons)
				weapon.Dispose();
			Weapons.Clear();
		}
	}
}
