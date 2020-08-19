using System.Collections.Generic;
using System.IO;
using System.Linq;
using WarriorsSnuggery.Maps;
using WarriorsSnuggery.Objects.Parts;

namespace WarriorsSnuggery
{
	public sealed class GameStatistics
	{
		// Paths
		public string Name { get; private set; }
		public string SaveName { get; private set; }
		// Changing Values
		public int Level;
		public int Money;
		public string Actor;
		public float RelativeHP;
		public int Mana;
		public int Kills;
		public int Deaths;
		public int MaxMana;

		public GameMode Mode;
		public GameType Type;
		public string MapType;
		public int Waves;
		public List<bool[]> Shroud;

		public readonly Dictionary<int, (float, float)> SpellCasters = new Dictionary<int, (float, float)>();

		public readonly List<string> UnlockedSpells = new List<string>();
		public readonly List<string> UnlockedActors = new List<string>();
		public readonly List<string> UnlockedTrophies = new List<string>();

		// Static Values
		public int FinalLevel;
		public int Difficulty;
		public int Seed;
		public bool Hardcore;

		// Script Values
		public string Script;
		public MiniTextNode[] ScriptValues;

		public GameStatistics(GameStatistics save)
		{
			Name = save.Name;
			SaveName = save.SaveName;

			Level = save.Level;
			Money = save.Money;
			Actor = save.Actor;
			RelativeHP = save.RelativeHP;
			Mana = save.Mana;
			Kills = save.Kills;
			Deaths = save.Deaths;

			Mode = save.Mode;
			Type = save.Type;
			MapType = save.MapType;
			Waves = save.Waves;
			Shroud = save.Shroud;

			SpellCasters = save.SpellCasters;

			UnlockedSpells = save.UnlockedSpells.ToList();
			UnlockedActors = save.UnlockedActors.ToList();
			UnlockedTrophies = save.UnlockedTrophies.ToList();

			FinalLevel = save.FinalLevel;
			Difficulty = save.Difficulty;
			MaxMana = save.MaxMana;
			Seed = save.Seed;
			Hardcore = save.Hardcore;

			Script = save.Script;
			ScriptValues = save.ScriptValues;
		}

		GameStatistics() { }

		public GameStatistics Copy()
		{
			return new GameStatistics(this);
		}

		public void Update(Game game)
		{
			if (game.World.PlayerSwitching)
				RelativeHP = ((PlayerSwitchPart)game.World.LocalPlayer.Parts.Find(p => p is PlayerSwitchPart)).RelativeHP;
			else
				RelativeHP = game.World.LocalPlayer.Health == null ? 1 : game.World.LocalPlayer.Health.RelativeHP;
		}

		public bool ActorAvailable(PlayablePartInfo playable)
		{
			return Program.IgnoreTech || playable.Unlocked || UnlockedActors.Contains(playable.InternalName);
		}

		public int CalculateScore()
		{
			// Positive Points
			var score = Level * 100 / FinalLevel;
			score += Kills * 5;
			score += Mana * 2;
			// Negative Points
			score -= Deaths * 25;
			return score;
		}

		public void Save(World world, bool withMap = true)
		{
			var scriptState = world.Game.GetScriptState(out Script);

			Mode = world.Game.Mode;
			Type = world.Game.Type;
			MapType = MapCreator.GetName(world.Map.Type, world.Game.Statistics);
			Waves = world.Game.CurrentWave();

			using (var writer = new StreamWriter(FileExplorer.Saves + SaveName + ".yaml", false))
			{
				writer.WriteLine("Name=" + Name);
				writer.WriteLine("Level=" + Level);
				writer.WriteLine("Difficulty=" + Difficulty);
				writer.WriteLine("Hardcore=" + Hardcore);
				writer.WriteLine("Money=" + Money);
				writer.WriteLine("LevelAim=" + FinalLevel);
				writer.WriteLine("MaxMana=" + MaxMana);
				writer.WriteLine("Kills=" + Kills);
				writer.WriteLine("Deaths=" + Deaths);
				writer.WriteLine("CurrentMode=" + Mode);
				writer.WriteLine("CurrentType=" + Type);
				writer.WriteLine("CurrentMapType=" + MapType);
				if (Waves != 0)
					writer.WriteLine("Waves=" + Waves);

				writer.WriteLine("Shroud=");
				for (int i = 0; i < Settings.MaxTeams; i++)
					writer.WriteLine("\t" + world.ShroudLayer.ToString(i));

				writer.WriteLine("SpellCasters=");
				for (int i = 0; i < world.Game.SpellManager.spellCasters.Length; i++)
				{
					var caster = world.Game.SpellManager.spellCasters[i];
					if (caster.Ready)
						continue;

					writer.WriteLine("\t" + "Caster=" + i);
					if (caster.RechargeProgress != 0f)
						writer.WriteLine("\t\tRecharge=" + caster.RechargeProgress);
					else
						writer.WriteLine("\t\tRemaining=" + caster.RemainingDuration);
				}

				writer.WriteLine("Seed=" + Seed);
				writer.WriteLine("Mana=" + Mana);
				writer.WriteLine("Actor=" + Actor);
				writer.WriteLine("\tHealth=" + RelativeHP);
				writer.WriteLine("UnlockedSpells=");
				foreach (var unlock in UnlockedSpells)
					writer.WriteLine("\t" + unlock + "=");
				writer.WriteLine("UnlockedActors=");
				foreach (var unlock in UnlockedActors)
					writer.WriteLine("\t" + unlock + "=");
				writer.WriteLine("UnlockedTrophies=");
				foreach (var unlock in UnlockedTrophies)
					writer.WriteLine("\t" + unlock + "=");

				if (scriptState != null)
				{
					writer.WriteLine("Script=" + Script);
					int i = 0;
					foreach(var obj in scriptState)
						writer.WriteLine("\t" + i++ + "=" + obj.ToString());
				}

				writer.Flush();
				writer.Close();
			}
			if (withMap)
				world.Save(FileExplorer.Saves, SaveName + "_map", true);
		}

		public void Delete()
		{
			File.Delete(FileExplorer.Saves + SaveName + ".yaml");

			if (File.Exists(FileExplorer.Saves + SaveName + "_map.yaml"))
				File.Delete(FileExplorer.Saves + SaveName + "_map.yaml");
		}

		public void SetName(string name)
		{
			Name = name;
			const string invalidChars = "#*+'?=!.:;,";
			foreach (var c in invalidChars)
				name = name.Replace(c, '-');

			SaveName = name;
		}

		public static GameStatistics CreateGameStatistic(int difficulty, bool hardcore, string name, int seed)
		{
			var statistic = new GameStatistics
			{
				Name = name,
				Hardcore = hardcore,
				Difficulty = difficulty
			};
			statistic.SetName(name);

			statistic.Level = 1;
			statistic.FinalLevel = (difficulty + 1) * 5;
			statistic.Money = 100 - difficulty * 10;
			statistic.MaxMana = GameSaveManager.DefaultStatistic.MaxMana;
			statistic.Actor = GameSaveManager.DefaultStatistic.Actor;
			statistic.Seed = seed;

			return statistic;
		}

		public static GameStatistics LoadGameStatistic(string name)
		{
			var statistic = new GameStatistics
			{
				SaveName = name
			};

			foreach (var node in RuleReader.FindAndRead(FileExplorer.Saves, name, ".yaml"))
			{
				switch (node.Key)
				{
					case "Name":
						statistic.Name = node.Convert<string>();
						break;
					case "Level":
						statistic.Level = node.Convert<int>();
						break;
					case "Difficulty":
						statistic.Difficulty = node.Convert<int>();
						break;
					case "Hardcore":
						statistic.Hardcore = node.Convert<bool>();
						break;
					case "Money":
						statistic.Money = node.Convert<int>();
						break;
					case "LevelAim":
						statistic.FinalLevel = node.Convert<int>();
						break;
					case "MaxMana":
						statistic.MaxMana = node.Convert<int>();
						break;
					case "Kills":
						statistic.Kills = node.Convert<int>();
						break;
					case "Deaths":
						statistic.Deaths = node.Convert<int>();
						break;
					case "CurrentMode":
						statistic.Mode = node.Convert<GameMode>();
						break;
					case "CurrentType":
						statistic.Type = node.Convert<GameType>();
						break;
					case "CurrentMapType":
						statistic.MapType = node.Value;
						break;
					case "Waves":
						statistic.Waves = node.Convert<int>();
						break;
					case "Shroud":
						statistic.Shroud = new List<bool[]>();

						foreach (var node2 in node.Children)
							statistic.Shroud.Add(node2.Convert<bool[]>());

						break;
					case "Seed":
						statistic.Seed = node.Convert<int>();
						break;
					case "Mana":
						statistic.Mana = node.Convert<int>();
						break;
					case "Actor":
						statistic.Actor = node.Convert<string>();

						foreach (var node2 in node.Children)
						{
							if (node2.Key == "Health")
								statistic.RelativeHP = node2.Convert<float>();
							else
								throw new YamlUnknownNodeException(node2.Key, name + ".yaml");
						}
						break;
					case "SpellCasters":

						foreach (var node2 in node.Children)
						{
							var id = node2.Convert<int>();

							var recharge = 0f;
							var duration = 0f;
							foreach (var node3 in node2.Children)
							{
								switch(node3.Key)
								{
									case "Recharge":
										recharge = node3.Convert<float>();

										break;
									case "Remaining":
										duration = node3.Convert<float>();

										break;
								}
							}

							statistic.SpellCasters.Add(id, (duration, recharge));
						}

						break;
					case "UnlockedSpells":
						foreach (var node2 in node.Children)
							statistic.UnlockedSpells.Add(node2.Key);

						break;
					case "UnlockedActors":
						foreach (var node2 in node.Children)
							statistic.UnlockedActors.Add(node2.Key);

						break;
					case "UnlockedTrophies":
						foreach (var node2 in node.Children)
							statistic.UnlockedTrophies.Add(node2.Key);

						break;
					case "Script":
						statistic.Script = node.Value;
						statistic.ScriptValues = node.Children.ToArray();

						break;
				}
			}

			return statistic;
		}
	}
}
