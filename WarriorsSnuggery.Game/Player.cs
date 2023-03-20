
using System;
using System.Collections.Generic;
using WarriorsSnuggery.Objects.Actors;
using WarriorsSnuggery.Objects.Actors.Parts;
using WarriorsSnuggery.Spells;

namespace WarriorsSnuggery
{
	public class Player
	{
        [Save]
        public byte Team;

        [Save]
		public int Money;

        [Save]
		public int Mana
		{
			get => mana;
			set => mana = Math.Clamp(value, 0, MaxMana);
		}
		int mana;
        [Save]
		public int MaxMana;

        [Save]
		public int Lifes
		{
			get => lifes;
			set => lifes = Math.Clamp(value, 0, MaxLifes);
		}
		int lifes;
        [Save]
		public int MaxLifes;
		public int NextLifePrice => 100 * lifes * lifes + 50;

        [Save]
		public int Kills;
        [Save]
		public int Deaths;

        [Save]
		public bool KeyFound;

        [Save]
		public readonly List<string> UnlockedSpells;
        [Save]
		public readonly List<string> UnlockedActors;
        [Save]
		public readonly List<string> UnlockedTrophies;

		public void AddSpell(SpellCasterType type)
		{
			if (HasUnlockedSpell(type))
				return;

			UnlockedSpells.Add(type.InnerName);
		}

		public bool HasUnlockedSpell(string innerName)
		{
			return HasUnlockedSpell(SpellCasterCache.Types[innerName]);
		}

		public bool HasUnlockedSpell(SpellCasterType type)
		{
			return Program.IgnoreTech || type.Unlocked || UnlockedSpells.Contains(type.InnerName);
		}

		public bool HasSpellAvailable(SpellCasterType type)
		{
			if (HasUnlockedSpell(type))
				return true;

			foreach (var before in type.Before)
			{
				if (string.IsNullOrEmpty(before))
					continue;

				if (!HasUnlockedSpell(before))
					return false;
			}

			return true;
		}

		public void UnlockActor(PlayablePartInfo playable)
		{
			if (UnlockedActors.Contains(playable.InternalName))
				return;

			UnlockedActors.Add(playable.InternalName);
		}

		public bool HasActorUnlocked(string innerName)
		{
			return HasActorUnlocked(ActorCache.Types[innerName].Playable);
		}

		public bool HasActorUnlocked(PlayablePartInfo playable)
		{
			return playable.Unlocked || Program.IgnoreTech || UnlockedActors.Contains(playable.InternalName);
		}

		public void UnlockTrophy(string name)
		{
			if (HasTrophyUnlocked(name))
				return;

			UnlockedTrophies.Add(name);
		}

		public bool HasTrophyUnlocked(string name)
		{
			return UnlockedTrophies.Contains(name);
		}
    }
}