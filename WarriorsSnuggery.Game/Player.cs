using System;
using System.Collections.Generic;
using System.Reflection;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Objects.Actors;
using WarriorsSnuggery.Objects.Actors.Parts;
using WarriorsSnuggery.Spells;

namespace WarriorsSnuggery
{
	public sealed class Player : ISaveable
	{
        [Save]
        public readonly byte Team;
        [Save]
        public readonly byte PlayerID;
        [Save]
        public readonly string Name;

        [Save, DefaultValue(0)]
		public int Money;
		public int Mana
		{
			get => mana;
			set => mana = Math.Clamp(value, 0, MaxMana);
		}
        [Save("Mana"), DefaultValue(0)]
		int mana;
        [Save]
		public int MaxMana;

		public int Lifes
		{
			get => lifes;
			set => lifes = Math.Clamp(value, 0, MaxLifes);
		}
        [Save("Lifes"), DefaultValue(0)]
		int lifes;
        [Save]
		public int MaxLifes;
		public int NextLifePrice => 100 * lifes * lifes + 50;

        [Save, DefaultValue(0)]
		public int Kills;
        [Save, DefaultValue(0)]
		public int Deaths;

		[Save]
		public string[] UnlockedSpells = new string[0];
        [Save]
		public string[] UnlockedActors = new string[0];
        [Save]
		public string[] UnlockedTrophies = new string[0];
		readonly List<string> unlockedSpells = new List<string>();
		readonly List<string> unlockedActors = new List<string>();
		readonly List<string> unlockedTrophies = new List<string>();

		// Saved separately
		public readonly SpellCasterManager SpellCasters;

		public Player(byte team, byte playerID, string name)
		{
			SpellCasters = new SpellCasterManager(this);
			Team = team;
			PlayerID = playerID;
			Name = name;
		}

		public void InitializeWith(GameSave save)
		{
			Money = 100 - save.Difficulty * 10;
			MaxMana = 200 - save.Difficulty * 10;

			MaxLifes = save.Hardcore ? 1 : 3;
			Lifes = MaxLifes;
		}

		public Player(TextNodeInitializer initializer)
		{
			SpellCasters = new SpellCasterManager(this);
			initializer.SetSaveFields(this);
			SpellCasters.Load(initializer.MakeInitializerWith(nameof(SpellCasters)));
		}

		Player(Player save)
		{
			// Copy all fields
			var fields = typeof(Player).GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
			foreach (var field in fields)
				field.SetValue(this, field.GetValue(save));

			unlockedSpells = new List<string>(UnlockedSpells);
			unlockedActors = new List<string>(UnlockedActors);
			unlockedTrophies = new List<string>(UnlockedTrophies);
		}

		public Player Clone()
		{
			return new Player(this);
		}

		public void AddSpell(SpellCasterType type)
		{
			if (HasSpellUnlocked(type))
				return;

			unlockedSpells.Add(type.InnerName);
			UnlockedSpells = unlockedSpells.ToArray();
		}

		public bool HasSpellUnlocked(string innerName)
		{
			return HasSpellUnlocked(SpellCasterCache.Types[innerName]);
		}

		public bool HasSpellUnlocked(SpellCasterType type)
		{
			return Program.IgnoreTech || type.Unlocked || unlockedSpells.Contains(type.InnerName);
		}

		public bool IsSpellUnlockable(SpellCasterType type)
		{
			if (HasSpellUnlocked(type))
				return true;

			foreach (var before in type.Before)
			{
				if (string.IsNullOrEmpty(before))
					continue;

				if (!HasSpellUnlocked(before))
					return false;
			}

			return true;
		}

		public void UnlockActor(PlayablePartInfo playable)
		{
			if (unlockedActors.Contains(playable.InternalName))
				return;

			unlockedActors.Add(playable.InternalName);
			UnlockedActors = unlockedActors.ToArray();
		}

		public bool HasActorUnlocked(string innerName)
		{
			return HasActorUnlocked(ActorCache.Types[innerName].Playable);
		}

		public bool HasActorUnlocked(PlayablePartInfo playable)
		{
			return playable.Unlocked || Program.IgnoreTech || unlockedActors.Contains(playable.InternalName);
		}

		public void UnlockTrophy(string name)
		{
			if (HasTrophyUnlocked(name))
				return;

			unlockedTrophies.Add(name);
			UnlockedTrophies = unlockedTrophies.ToArray();
		}

		public bool HasTrophyUnlocked(string name)
		{
			return unlockedTrophies.Contains(name);
		}

		public void IncreaseDeathCount()
		{
			if (Lifes > 0)
			{
				Lifes--;
				return;
			}

			Deaths++;
		}

		public TextNodeSaver Save()
		{
			var saver = new TextNodeSaver();
			saver.AddSaveFields(this);
			saver.AddChildren(nameof(SpellCasters), SpellCasters.Save());

			return saver;
		}
    }
}