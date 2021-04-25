using System;
using System.Collections.Generic;
using System.IO;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Maps;
using WarriorsSnuggery.Objects;
using WarriorsSnuggery.Objects.Parts;
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

		public int Kills;
		public int Deaths;

		public bool KeyFound;

		internal readonly Dictionary<int, (float, float)> spellCasters;

		internal readonly List<string> unlockedSpells;
		internal readonly List<string> unlockedActors;
		internal readonly List<string> unlockedTrophies;

		public GameStats(GameSave save)
		{
			Money = save.Money;
			mana = save.Mana;
			MaxMana = save.MaxMana;
			Kills = save.Kills;
			Deaths = save.Deaths;

			KeyFound = save.KeyFound;

			spellCasters = new Dictionary<int, (float, float)>(save.SpellCasters);

			unlockedSpells = new List<string>();
			if (save.UnlockedSpells != null)
				unlockedSpells.AddRange(save.UnlockedSpells);

			unlockedActors = new List<string>();
			if (save.UnlockedActors != null)
				unlockedActors.AddRange(save.UnlockedActors);

			unlockedTrophies = new List<string>();
			if (save.UnlockedTrophies != null)
				unlockedTrophies.AddRange(save.UnlockedTrophies);
		}

		public (float, float) GetSpellCaster(int i)
		{
			if (!spellCasters.ContainsKey(i))
				return (0f, 0f);

			return spellCasters[i];
		}

		public void AddSpell(SpellTreeNode node)
		{
			if (SpellUnlocked(node))
				return;

			unlockedSpells.Add(node.InnerName);
		}

		public bool SpellUnlocked(SpellTreeNode node)
		{
			return SpellUnlocked(node.InnerName);
		}

		public bool SpellUnlocked(string innerName)
		{
			return unlockedSpells.Contains(innerName);
		}

		public void AddActor(PlayablePartInfo playable)
		{
			if (unlockedActors.Contains(playable.InternalName))
				return;

			unlockedActors.Add(playable.InternalName);
		}

		public bool ActorAvailable(PlayablePartInfo playable)
		{
			return Program.IgnoreTech || playable.Unlocked || unlockedActors.Contains(playable.InternalName);
		}

		public void AddTrophy(string name)
		{
			if (TrophyUnlocked(name))
				return;

			unlockedTrophies.Add(name);
		}

		public bool TrophyUnlocked(string name)
		{
			return unlockedTrophies.Contains(name);
		}
	}

	public sealed class GameSave
	{
		public string Name { get; private set; }
		public string SaveName { get; private set; }
		public string MapSaveName => SaveName + "_map";

		// Changing Values
		public int Level { get; private set; }
		public int Money { get; private set; }
		public string Actor { get; private set; }
		public float Health { get; private set; }
		public int Mana { get; private set; }
		public int Kills { get; private set; }
		public int Deaths { get; private set; }
		public int MaxMana { get; private set; }

		public Dictionary<int, (float, float)> SpellCasters { get; private set; }

		public string[] UnlockedSpells { get; private set; }
		public string[] UnlockedActors { get; private set; }
		public string[] UnlockedTrophies { get; private set; }

		// Level Values
		public ObjectiveType CurrentObjective { get; private set; }
		public MissionType CurrentMission { get; private set; }
		public MapType CurrentMapType { get; private set; }
		public int Waves { get; private set; }
		public bool KeyFound { get; private set; }
		public List<bool[]> Shroud { get; private set; }

		// Static Values
		public int FinalLevel { get; private set; }
		public int Difficulty { get; private set; }
		public int Seed { get; private set; }
		public bool Hardcore { get; private set; }

		// Script Values
		public string Script { get; private set; }
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
			SpellCasters = new Dictionary<int, (float, float)>(save.SpellCasters);
		}

		public GameSave Copy()
		{
			return new GameSave(this);
		}

		public GameSave(string file) : this()
		{
			SaveName = file;

			var properties = typeof(GameSave).GetProperties();

			foreach (var node in TextNodeLoader.FromFile(FileExplorer.Saves, file + ".yaml"))
			{
				switch (node.Key)
				{
					case nameof(Shroud):
						Shroud = new List<bool[]>();

						foreach (var node2 in node.Children)
							Shroud.Add(node2.Convert<bool[]>());

						break;
					case nameof(CurrentMapType):
						CurrentMapType = MapCreator.GetType(node.Value);

						break;
					case nameof(SpellCasters):

						foreach (var node2 in node.Children)
						{
							var id = node2.Convert<int>();

							var recharge = 0f;
							var duration = 0f;
							foreach (var node3 in node2.Children)
							{
								switch (node3.Key)
								{
									case "Recharge":
										recharge = node3.Convert<float>();

										break;
									case "Remaining":
										duration = node3.Convert<float>();

										break;
								}
							}

							SpellCasters.Add(id, (duration, recharge));
						}

						break;
					case nameof(Script):
						Script = node.Convert<string>();
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
			SetName(name);

			Hardcore = hardcore;
			Difficulty = difficulty;

			Level = 1;
			FinalLevel = (difficulty + 1) * 5;
			Money = 100 - difficulty * 10;
			MaxMana = GameSaveManager.DefaultSave.MaxMana;
			Actor = GameSaveManager.DefaultSave.Actor;
			Seed = seed;
		}

		GameSave()
		{
			SpellCasters = new Dictionary<int, (float, float)>();
		}

		public int CalculateScore()
		{
			// Positive Points
			var score = Level * 100 / FinalLevel;
			score += Kills * 5;
			score += Money * 2;
			// Negative Points
			score -= Deaths * 25;
			return score;
		}

		public void IncreaseDeathCount()
		{
			Deaths++;
		}

		public void Save(Game game)
		{
			Update(game);

			var scriptState = game.GetScriptState(out var scriptName);
			Script = scriptName;

			using (var writer = new StreamWriter(FileExplorer.Saves + SaveName + ".yaml", false))
			{
				writer.WriteLine($"{nameof(Name)}= {Name}");
				writer.WriteLine($"{nameof(Level)}= {Level}");
				writer.WriteLine($"{nameof(Difficulty)}= {Difficulty}");
				writer.WriteLine($"{nameof(Hardcore)}= {Hardcore}");
				writer.WriteLine($"{nameof(Money)}= {Money}");
				writer.WriteLine($"{nameof(FinalLevel)}= {FinalLevel}");
				writer.WriteLine($"{nameof(MaxMana)}= {MaxMana}");
				writer.WriteLine($"{nameof(Kills)}= {Kills}");
				writer.WriteLine($"{nameof(Deaths)}= {Deaths}");
				writer.WriteLine($"{nameof(CurrentObjective)}= {CurrentObjective}");
				writer.WriteLine($"{nameof(CurrentMission)}= {CurrentMission}");
				writer.WriteLine($"{nameof(CurrentMapType)}= {MapCreator.GetName(CurrentMapType)}");
				if (Waves != 0)
					writer.WriteLine($"{nameof(Waves)}= {Waves}");
				if (KeyFound)
					writer.WriteLine($"{nameof(KeyFound)}= {KeyFound}");

				writer.WriteLine($"{nameof(Shroud)}=");
				for (int i = 0; i < Settings.MaxTeams; i++)
					writer.WriteLine("\t" + game.World.ShroudLayer.ToString(i));

				writer.WriteLine($"{nameof(SpellCasters)}=");
				for (int i = 0; i < game.SpellManager.spellCasters.Length; i++)
				{
					var caster = game.SpellManager.spellCasters[i];
					if (caster.Ready)
						continue;

					writer.WriteLine("\t" + "Caster= " + i);
					if (caster.RechargeProgress != 0f)
						writer.WriteLine("\t\tRecharge= " + caster.RechargeProgress);
					else
						writer.WriteLine("\t\tRemaining= " + caster.RemainingDuration);
				}

				writer.WriteLine($"{nameof(Seed)}= {Seed}");
				writer.WriteLine($"{nameof(Mana)}= {Mana}");
				writer.WriteLine($"{nameof(Actor)}= {Actor}");
				writer.WriteLine($"{nameof(Health)}= {Health}");

				writer.Write($"{nameof(UnlockedSpells)}={string.Join(',', UnlockedSpells)}");
				writer.Write($"{nameof(UnlockedActors)}={string.Join(',', UnlockedActors)}");
				writer.Write($"{nameof(UnlockedTrophies)}={string.Join(',', UnlockedTrophies)}");

				if (!string.IsNullOrEmpty(Script))
				{
					writer.WriteLine($"{nameof(Script)}= {Script}");
					for (var i = 0; i < scriptState.Length; i++)
						writer.WriteLine("\t" + i + "= " + scriptState[i].ToString());
				}

				writer.Flush();
				writer.Close();
			}

			game.World.SaveMap(FileExplorer.Saves, MapSaveName, true);
		}

		public void Update(Game game, bool levelIncrease = false)
		{
			if (levelIncrease)
				Level++;

			var player = game.World.LocalPlayer;

			Actor = ActorCreator.GetName(player.Type);

			if (player.IsPlayerSwitch)
				Health = ((PlayerSwitchPart)player.Parts.Find(p => p is PlayerSwitchPart)).RelativeHP;
			else
				Health = player.Health == null ? 1 : player.Health.RelativeHP;

			CurrentObjective = game.ObjectiveType;
			CurrentMission = game.MissionType;

			var save = game.Save;
			var mapType = game.MapType;
			CurrentMapType = mapType.IsSave ? save.CurrentMapType : mapType;

			Waves = game.CurrentWave;

			var stats = game.Stats;

			KeyFound = stats.KeyFound;
			Money = stats.Money;
			Mana = stats.Mana;
			MaxMana = stats.MaxMana;
			Deaths = stats.Deaths;
			Kills = stats.Kills;

			SpellCasters = new Dictionary<int, (float, float)>(stats.spellCasters);

			UnlockedSpells = stats.unlockedSpells.ToArray();
			UnlockedActors = stats.unlockedActors.ToArray();
			UnlockedTrophies = stats.unlockedTrophies.ToArray();
		}

		public void Delete()
		{
			if (File.Exists(FileExplorer.Saves + SaveName + ".yaml"))
				File.Delete(FileExplorer.Saves + SaveName + ".yaml");

			if (File.Exists(FileExplorer.Saves + MapSaveName + ".yaml"))
				File.Delete(FileExplorer.Saves + MapSaveName + ".yaml");
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
