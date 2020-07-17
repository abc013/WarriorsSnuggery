using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Objects.Weapons;

namespace WarriorsSnuggery
{
	public class WeaponLayer
	{
		// There are not many weapons ingame, so no sectors are needed
		public readonly List<Weapon> Weapons = new List<Weapon>();
		public readonly List<Weapon> VisibleWeapons = new List<Weapon>();

		readonly List<Weapon> weaponsToRemove = new List<Weapon>();
		readonly List<Weapon> weaponsToAdd = new List<Weapon>();

		bool firstTick = true;

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
			if (weaponsToAdd.Any())
			{
				foreach (var weapon in weaponsToAdd)
					if (weapon.CheckVisibility())
						VisibleWeapons.Add(weapon);
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
				{
					Weapons.Remove(weapon);
					if (weapon.CheckVisibility())
						VisibleWeapons.Remove(weapon);
				}
				weaponsToRemove.Clear();
			}
		}

		public void CheckVisibility()
		{
			foreach (var w in Weapons)
				w.CheckVisibility();
			VisibleWeapons.Clear();
			VisibleWeapons.AddRange(Weapons);
		}

		public void CheckVisibility(CPos topLeft, CPos bottomRight)
		{
			VisibleWeapons.Clear();

			foreach (var w in Weapons.Where(a => a.GraphicPosition.X > topLeft.X && a.GraphicPosition.X < bottomRight.X && a.GraphicPosition.Y > topLeft.Y && a.GraphicPosition.Y < bottomRight.Y))
			{
				if (w.CheckVisibility())
					VisibleWeapons.Add(w);
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
