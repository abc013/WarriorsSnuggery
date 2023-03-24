using System.Collections.Generic;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Objects.Weapons;

namespace WarriorsSnuggery.Maps.Layers
{
	public sealed class WeaponLayer : ISaveable
	{
		// There are not many weapons ingame, so no sectors are needed
		public readonly List<Weapon> Weapons = new List<Weapon>();

		readonly List<Weapon> weaponsToRemove = new List<Weapon>();
		readonly List<Weapon> weaponsToAdd = new List<Weapon>();

		public WeaponLayer() { }

		public void Add(Weapon weapon)
		{
			weaponsToAdd.Add(weapon);
		}

		public void Remove(Weapon weapon)
		{
			weaponsToRemove.Add(weapon);
		}

		public void Tick()
		{
			if (weaponsToAdd.Count != 0)
			{
				Weapons.AddRange(weaponsToAdd);
				weaponsToAdd.Clear();
			}

			foreach (var weapon in Weapons)
				weapon.Tick();

			if (weaponsToRemove.Count != 0)
			{
				foreach (var weapon in weaponsToRemove)
					Weapons.Remove(weapon);
				weaponsToRemove.Clear();
			}
		}

		public HashSet<Weapon> GetVisible(CPos topleft, CPos bottomright)
		{
			var visibleWeapons = new HashSet<Weapon>();

			foreach (var weapon in Weapons)
			{
				if (weapon is BeamWeapon)
				{
					visibleWeapons.Add(weapon);
					continue;
				}

				if (weapon.Position.X >= bottomright.X || weapon.Position.X < topleft.X)
					continue;

				if (weapon.Position.Y >= bottomright.Y || weapon.Position.Y < topleft.Y)
					continue;

				visibleWeapons.Add(weapon);
			}

			return visibleWeapons;
		}

		public TextNodeSaver Save()
		{
			var saver = new TextNodeSaver();
			for (int i = 0; i < Weapons.Count; i++)
				saver.AddChildren($"{i}", Weapons[i].Save());

			return saver;
		}
	}
}
