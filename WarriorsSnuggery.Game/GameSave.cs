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
		public int GameSaveFormat { get; private set; }
		public string Name { get; private set; }
		public string SaveName { get; private set; }
		public string MapSaveName => SaveName + "_map";

		public string[] ActiveMods { get; private set; }

		// Changing Values
		public int Level { get; private set; }
		public int Money { get; private set; }
		public int Kills { get; private set; }
		public int Deaths { get; private set; }

		public string Actor { get; private set; }
		public float Health { get; private set; }

		public int Mana { get; private set; }
		public int MaxMana { get; private set; }

		public int Lives { get; private set; }
		public int MaxLives { get; private set; }

		public Dictionary<string, (int duration, int recharge)> SpellCasters { get; private set; }

		public string[] UnlockedSpells { get; private set; }
		public string[] UnlockedActors { get; private set; }
		public string[] UnlockedTrophies { get; private set; }

		// Level Values
		public ObjectiveType CurrentObjective { get; private set; }
		public MissionType CurrentMission { get; private set; }
		public MapType CurrentMapType { get; private set; }
		public Color CurrentAmbience { get; private set; }
		public int Waves { get; private set; }
		public bool KeyFound { get; private set; }
		public Dictionary<byte, bool[]> Shroud { get; private set; }
		public Dictionary<string, bool> CustomConditions { get; private set; }

		// Static Values
		public int FinalLevel { get; private set; }
		public int Difficulty { get; private set; }
		public int Seed { get; private set; }
		public bool Hardcore { get; private set; }

		// Script Values
		public PackageFile Script { get; private set; }
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
			ActiveMods = new string[0];
			CurrentAmbience = Color.White;

			var properties = typeof(GameSave).GetProperties();

			foreach (var node in TextNodeLoader.FromFilepath(filepath))
			{
				switch (node.Key)
				{
					case nameof(Shroud):
						Shroud = new Dictionary<byte, bool[]>();

						foreach (var node2 in node.Children)
							Shroud.Add(byte.Parse(node2.Key), node2.Convert<bool[]>());

						break;
					case nameof(CustomConditions):
						CustomConditions = new Dictionary<string, bool>();

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
					case nameof(Script):
						Script = node.Convert<PackageFile>();
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

		GameSave()
		{
			SpellCasters = new Dictionary<string, (int, int)>();
		}

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

			var scriptState = game.GetScriptState(out var scriptName);
			Script = scriptName;

			using (var writer = new StreamWriter(FileExplorer.Saves + SaveName + ".yaml", false))
			{
				writer.WriteLine($"{nameof(GameSaveFormat)}={Constants.CurrentGameSaveFormat}");
				writer.WriteLine($"{nameof(Name)}={Name}");
				writer.WriteLine($"{nameof(ActiveMods)}={string.Join(',', ActiveMods)}");
				writer.WriteLine($"{nameof(Level)}={Level}");
				writer.WriteLine($"{nameof(Difficulty)}={Difficulty}");
				writer.WriteLine($"{nameof(Hardcore)}={Hardcore}");
				writer.WriteLine($"{nameof(Money)}={Money}");
				writer.WriteLine($"{nameof(FinalLevel)}={FinalLevel}");
				writer.WriteLine($"{nameof(MaxMana)}={MaxMana}");
				writer.WriteLine($"{nameof(Kills)}={Kills}");
				writer.WriteLine($"{nameof(Deaths)}={Deaths}");
				writer.WriteLine($"{nameof(Lives)}={Lives}");
				writer.WriteLine($"{nameof(MaxLives)}={MaxLives}");
				writer.WriteLine($"{nameof(CurrentObjective)}={CurrentObjective}");
				writer.WriteLine($"{nameof(CurrentMission)}={CurrentMission}");
				writer.WriteLine($"{nameof(CurrentMapType)}={MapCache.Types[CurrentMapType]}");
				if (CurrentAmbience != Color.White)
				{
					var color = CurrentAmbience.ToSysColor();
					writer.WriteLine($"{nameof(CurrentAmbience)}={color.R}, {color.G}, {color.B}, {color.A}");
				}
				if (Waves != 0)
					writer.WriteLine($"{nameof(Waves)}={Waves}");
				if (KeyFound)
					writer.WriteLine($"{nameof(KeyFound)}={KeyFound}");

				writer.WriteLine($"{nameof(Shroud)}=");
				foreach (var team in game.World.ShroudLayer.UsedTeams)
					writer.WriteLine("\t" + game.World.ShroudLayer.ToString(team));

				writer.WriteLine($"{nameof(CustomConditions)}=");
				foreach (var condition in game.ConditionManager.SaveConditions())
					writer.WriteLine(condition);

				writer.WriteLine($"{nameof(SpellCasters)}=");
				foreach (var caster in game.SpellManager.Casters)
				{
					foreach (var @string in caster.Save())
						writer.WriteLine("\t" + @string);
				}

				writer.WriteLine($"{nameof(Seed)}={Seed}");
				writer.WriteLine($"{nameof(Mana)}={Mana}");
				writer.WriteLine($"{nameof(Actor)}={Actor}");
				writer.WriteLine($"{nameof(Health)}={Health}");

				writer.WriteLine($"{nameof(UnlockedSpells)}={string.Join(',', UnlockedSpells)}");
				writer.WriteLine($"{nameof(UnlockedActors)}={string.Join(',', UnlockedActors)}");
				writer.WriteLine($"{nameof(UnlockedTrophies)}={string.Join(',', UnlockedTrophies)}");

				if (Script != null)
				{
					writer.WriteLine($"{nameof(Script)}={Script}");
					for (var i = 0; i < scriptState.Length; i++)
						writer.WriteLine($"\t{i}={scriptState[i]}");
				}

				writer.Flush();
				writer.Close();
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

			Waves = game.CurrentWave;

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
