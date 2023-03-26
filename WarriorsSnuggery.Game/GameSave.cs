using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Maps;
using WarriorsSnuggery.Maps.Pieces;
using WarriorsSnuggery.Objects.Actors;
using WarriorsSnuggery.Objects.Actors.Parts;
using WarriorsSnuggery.Spells;

namespace WarriorsSnuggery
{
	public sealed class GameStats
	{
		public int Money;

		public int Mana
		{
			get => mana;
			set => mana = Math.Clamp(value, 0, MaxMana);
		}
		int mana;
		public int MaxMana;

		public int Lifes
		{
			get => lifes;
			set => lifes = Math.Clamp(value, 0, MaxLifes);
		}
		int lifes;
		public int MaxLifes;

		public int Kills;
		public int Deaths;

		public bool KeyFound;

		public readonly List<string> UnlockedSpells;
		public readonly List<string> UnlockedActors;
		public readonly List<string> UnlockedTrophies;

		public GameStats(GameSave save)
		{
			Money = save.Money;
			mana = save.Mana;
			MaxMana = save.MaxMana;
			Kills = save.Kills;
			Deaths = save.Deaths;
			lifes = save.Lives;
			MaxLifes = save.MaxLives;

			KeyFound = save.KeyFound;

			UnlockedSpells = new List<string>();
			if (save.UnlockedSpells != null)
				UnlockedSpells.AddRange(save.UnlockedSpells);

			UnlockedActors = new List<string>();
			if (save.UnlockedActors != null)
				UnlockedActors.AddRange(save.UnlockedActors);

			UnlockedTrophies = new List<string>();
			if (save.UnlockedTrophies != null)
				UnlockedTrophies.AddRange(save.UnlockedTrophies);
		}

		public void AddSpell(SpellCasterType type)
		{
			if (SpellUnlocked(type))
				return;

			UnlockedSpells.Add(type.InnerName);
		}

		public bool SpellUnlocked(string innerName)
		{
			return SpellUnlocked(SpellCasterCache.Types[innerName]);
		}

		public bool SpellUnlocked(SpellCasterType type)
		{
			return Program.IgnoreTech || type.Unlocked || UnlockedSpells.Contains(type.InnerName);
		}

		public bool SpellAvailable(SpellCasterType type)
		{
			if (SpellUnlocked(type))
				return true;

			foreach (var before in type.Before)
			{
				if (string.IsNullOrEmpty(before))
					continue;

				if (!SpellUnlocked(before))
					return false;
			}

			return true;
		}

		public void AddActor(PlayablePartInfo playable)
		{
			if (UnlockedActors.Contains(playable.InternalName))
				return;

			UnlockedActors.Add(playable.InternalName);
		}

		public bool ActorUnlocked(string innerName)
		{
			return ActorUnlocked(ActorCache.Types[innerName].Playable);
		}

		public bool ActorUnlocked(PlayablePartInfo playable)
		{
			return playable.Unlocked || Program.IgnoreTech || UnlockedActors.Contains(playable.InternalName);
		}

		public void AddTrophy(string name)
		{
			if (TrophyUnlocked(name))
				return;

			UnlockedTrophies.Add(name);
		}

		public bool TrophyUnlocked(string name)
		{
			return UnlockedTrophies.Contains(name);
		}

		public int NextLifePrice()
		{
			return 100 * lifes * lifes + 50;
		}
	}

	public sealed class GameSave
	{
		// Saved separately
		public int GameSaveFormat { get; private set; }
		[Save]
		public string Name { get; private set; }
		public string SaveName { get; private set; }
		public string MapSaveName => SaveName + "_map";

		[Save]
		public string[] ActiveMods { get; private set; } = new string[0];

		// Changing Values
		[Save]
		public int Level { get; private set; }
		[Save]
		public int Money { get; private set; }
		[Save]
		public int Kills { get; private set; }
		[Save]
		public int Deaths { get; private set; }

		[Save]
		public string Actor { get; private set; }
		[Save]
		public float Health { get; private set; }

		[Save]
		public int Mana { get; private set; }
		[Save]
		public int MaxMana { get; private set; }

		[Save]
		public int Lives { get; private set; }
		[Save]
		public int MaxLives { get; private set; }

		// Saved separately
		public Dictionary<string, (int duration, int recharge)> SpellCasters { get; private set; } = new Dictionary<string, (int, int)>();

		[Save]
		public string[] UnlockedSpells { get; private set; }
		[Save]
		public string[] UnlockedActors { get; private set; }
		[Save]
		public string[] UnlockedTrophies { get; private set; }

		// Level Values
		[Save]
		public ObjectiveType CurrentObjective { get; private set; }
		[Save]
		public MissionType CurrentMission { get; private set; }
		// Saved separately
		public MapType CurrentMapType { get; private set; }
		// TODO: save via [Save] attribute
		public Color CurrentAmbience { get; private set; } = Color.White;
		[Save, DefaultValue(0)]
		public int Waves { get; private set; }
		[Save, DefaultValue(false)]
		public bool KeyFound { get; private set; }
		// Saved separately
		public Dictionary<string, bool> CustomConditions { get; private set; } = new Dictionary<string, bool>();

		// Static Values
		[Save]
		public int FinalLevel { get; private set; }
		[Save]
		public int Difficulty { get; private set; }
		[Save]
		public int Seed { get; private set; }
		[Save, DefaultValue(false)]
		public bool Hardcore { get; private set; }

		// Script Values
		[Save, DefaultValue(null)]
		public PackageFile Script { get; private set; }
		// Saved separately
		public TextNode[] ScriptState { get; private set; }

		GameSave(GameSave save)
		{
			// Copy all fields
			var fields = typeof(GameSave).GetProperties();
			foreach (var properties in fields)
			{
				if (properties.SetMethod != null)
					properties.SetValue(this, properties.GetValue(save));
			}

			// Create new dictionary and lists
			SpellCasters = new Dictionary<string, (int, int)>(save.SpellCasters);
		}

		public GameSave Copy()
		{
			return new GameSave(this);
		}

		public GameSave(string filepath) : this()
		{
			SaveName = FileExplorer.FileName(filepath);

			var properties = typeof(GameSave).GetProperties();

			foreach (var node in TextNodeLoader.FromFilepath(filepath))
			{
				switch (node.Key)
				{
					case nameof(CustomConditions):
						foreach (var node2 in node.Children)
							CustomConditions.Add(node2.Key, node2.Convert<bool>());

						break;
					case nameof(CurrentMapType):
						CurrentMapType = MapCache.Types[node.Value];

						break;
					case nameof(SpellCasters):
						foreach (var node2 in node.Children)
						{
							var innerName = node2.Key;

							var recharge = 0;
							var duration = 0;
							foreach (var node3 in node2.Children)
							{
								switch (node3.Key)
								{
									case "Recharge":
										recharge = node3.Convert<int>();

										break;
									case "Duration":
										duration = node3.Convert<int>();

										break;
								}
							}

							SpellCasters.Add(innerName, (duration, recharge));
						}

						break;
					case nameof(ScriptState):
						ScriptState = node.Children.ToArray();

						break;
					default:
						TypeLoader.SetValue(this, properties, node);

						break;
				}
			}
		}

		public GameSave(int difficulty, bool hardcore, string name, int seed) : this()
		{
			GameSaveFormat = Constants.CurrentGameSaveFormat;
			SetName(name);

			ActiveMods = PackageManager.ActivePackages.Select(p => p.InternalName).ToArray();

			Hardcore = hardcore;
			Difficulty = difficulty;

			Level = 1;
			FinalLevel = (difficulty + 1) * 5;
			Money = 100 - difficulty * 10;
			MaxMana = GameSaveManager.DefaultSave.MaxMana;
			Actor = GameSaveManager.DefaultSave.Actor;
			Seed = seed;

			Lives = hardcore ? 1 : 3;
			MaxLives = Lives;
		}

		GameSave() { }

		public (int duration, int recharge) GetSpellCasterValues(string innerName)
		{
			if (!SpellCasters.ContainsKey(innerName))
				return (0, 0);

			return SpellCasters[innerName];
		}

		public int CalculateScore()
		{
			// Positive Points
			var score = Level * 100 / FinalLevel;
			score += Kills * 5;
			score += Money * 2;
			score += Lives * 25;
			// Negative Points
			score -= Deaths * 25;
			return score;
		}

		public void IncreaseDeathCount()
		{
			if (Lives > 0)
				Lives--;

			Deaths++;
		}

		public void Save(Game game)
		{
			Update(game);

			var saver = new TextNodeSaver();
			saver.Add(nameof(GameSaveFormat), Constants.CurrentGameSaveFormat);
			saver.Add(nameof(CurrentMapType), MapCache.Types[CurrentMapType]);
			saver.Add(nameof(CurrentAmbience), $"{(int)(CurrentAmbience.R * 255)}, {(int)(CurrentAmbience.G * 255)}, {(int)(CurrentAmbience.B * 255)}, {(int)(CurrentAmbience.A * 255)}");

			if (game.Script != null)
			{
				Script = game.Script.PackageFile;
				saver.AddChildren(nameof(ScriptState), game.Script.Save(), true);
			}

			saver.AddSaveFields(this);

			saver.AddChildren(nameof(SpellCasters), game.SpellManager.Save(), true);
			saver.AddChildren(nameof(CustomConditions), game.ConditionManager.Save(), true);

			using (var writer = new StreamWriter(FileExplorer.Saves + SaveName + ".yaml", false))
			{
				foreach (var savedString in saver.GetStrings())
					writer.WriteLine(savedString);
			}

			PieceSaver.SaveWorld(game.World, FileExplorer.Saves, MapSaveName, true);
		}

		public void Update(Game game, bool levelIncrease = false)
		{
			if (levelIncrease)
				Level++;

			ActiveMods = PackageManager.ActivePackages.Select(p => p.InternalName).ToArray();

			var player = game.World.LocalPlayer;

			Actor = ActorCache.Types[player.Type];

			if (player.IsPlayerSwitch)
				Health = player.GetPart<PlayerSwitchPart>().RelativeHP;
			else
				Health = player.Health == null ? 1 : player.Health.RelativeHP;

			CurrentObjective = game.ObjectiveType;
			CurrentMission = game.MissionType;

			var save = game.Save;
			var mapType = game.MapType;
			CurrentMapType = mapType.IsSave ? save.CurrentMapType : mapType;
			CurrentAmbience = WorldRenderer.Ambient;

			Waves = game.WaveController == null ? 0 : game.WaveController.CurrentWave;

			var stats = game.Stats;

			KeyFound = !levelIncrease && stats.KeyFound;
			Money = stats.Money;
			Mana = stats.Mana;
			MaxMana = stats.MaxMana;
			Deaths = stats.Deaths;
			Kills = stats.Kills;
			Lives = stats.Lifes;
			MaxLives = stats.MaxLifes;

			UnlockedSpells = stats.UnlockedSpells.ToArray();
			UnlockedActors = stats.UnlockedActors.ToArray();
			UnlockedTrophies = stats.UnlockedTrophies.ToArray();
		}

		public void Delete()
		{
			if (File.Exists(FileExplorer.Saves + SaveName + ".yaml"))
				File.Delete(FileExplorer.Saves + SaveName + ".yaml");

			if (File.Exists(FileExplorer.Saves + MapSaveName + ".yaml"))
				File.Delete(FileExplorer.Saves + MapSaveName + ".yaml");
		}

		public bool OutdatedVersion()
		{
			return GameSaveFormat < Constants.CurrentGameSaveFormat;
		}

		public bool InvalidMods()
		{
			// string comparison. Order of mods may be important.
			return string.Join(',', ActiveMods) != string.Join(',', PackageManager.ActivePackages.Select(p => p.InternalName));
		}

		public void SetName(string name)
		{
			Name = name;
			foreach (var c in FileExplorer.InvalidFileChars)
				name = name.Replace(c, '_');

			SaveName = name;
		}
	}
}
