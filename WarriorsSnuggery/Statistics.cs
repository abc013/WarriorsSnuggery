using System.Collections.Generic;
using System.IO;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Maps;
using WarriorsSnuggery.Objects.Parts;

namespace WarriorsSnuggery
{
	public sealed class GameStatistics
	{
		// Paths
		public string Name;
		public string SaveName;
		// Changing Values
		public int Level;
		public int Money;
		public string Actor;
		public float Health;
		public int Mana;
		public int Kills;
		public int Deaths;
		public int MaxMana;

		public GameMode CurrentMode;
		public GameType CurrentType;
		public string CurrentMapType;
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
			Health = save.Health;
			Mana = save.Mana;
			Kills = save.Kills;
			Deaths = save.Deaths;

			CurrentMode = save.CurrentMode;
			CurrentType = save.CurrentType;
			CurrentMapType = save.CurrentMapType;
			Waves = save.Waves;
			Shroud = save.Shroud;

			SpellCasters = new Dictionary<int, (float, float)>(save.SpellCasters);

			UnlockedSpells = new List<string>(save.UnlockedSpells);
			UnlockedActors = new List<string>(save.UnlockedActors);
			UnlockedTrophies = new List<string>(save.UnlockedTrophies);

			FinalLevel = save.FinalLevel;
			Difficulty = save.Difficulty;
			MaxMana = save.MaxMana;
			Seed = save.Seed;
			Hardcore = save.Hardcore;

			Script = save.Script;
			ScriptValues = save.ScriptValues;
		}

		public GameStatistics(string file)
		{
			SaveName = file;

			var fields = PartLoader.GetFields(this, false);

			foreach (var node in RuleReader.FromFile(FileExplorer.FindPath(FileExplorer.Saves, file, ".yaml"), file + ".yaml"))
			{
				switch (node.Key)
				{
					case nameof(Shroud):
						Shroud = new List<bool[]>();

						foreach (var node2 in node.Children)
							Shroud.Add(node2.Convert<bool[]>());

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
					case nameof(UnlockedSpells):
						foreach (var node2 in node.Children)
							UnlockedSpells.Add(node2.Key);

						break;
					case nameof(UnlockedActors):
						foreach (var node2 in node.Children)
							UnlockedActors.Add(node2.Key);

						break;
					case nameof(UnlockedTrophies):
						foreach (var node2 in node.Children)
							UnlockedTrophies.Add(node2.Key);

						break;
					case nameof(Script):
						Script = node.Convert<string>();
						ScriptValues = node.Children.ToArray();

						break;
					default:
						PartLoader.SetValue(this, fields, node);

						break;
				}
			}
		}

		public GameStatistics(int difficulty, bool hardcore, string name, int seed)
		{
			SetName(name);

			Hardcore = hardcore;
			Difficulty = difficulty;

			Level = 1;
			FinalLevel = (difficulty + 1) * 5;
			Money = 100 - difficulty * 10;
			MaxMana = GameSaveManager.DefaultStatistic.MaxMana;
			Actor = GameSaveManager.DefaultStatistic.Actor;
			Seed = seed;
		}

		public GameStatistics Copy()
		{
			return new GameStatistics(this);
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
			score += Money * 2;
			// Negative Points
			score -= Deaths * 25;
			return score;
		}

		public void Save(World world, bool withMap = true)
		{
			var scriptState = world.Game.GetScriptState(out Script);

			if (world.PlayerSwitching)
				Health = ((PlayerSwitchPart)world.LocalPlayer.Parts.Find(p => p is PlayerSwitchPart)).RelativeHP;
			else
				Health = world.LocalPlayer.Health == null ? 1 : world.LocalPlayer.Health.RelativeHP;

			CurrentMode = world.Game.Mode;
			CurrentType = world.Game.Type;
			CurrentMapType = MapCreator.GetName(world.Map.Type, world.Game.Statistics);
			Waves = world.Game.CurrentWave();

			using (var writer = new StreamWriter(FileExplorer.Saves + SaveName + ".yaml", false))
			{
				writer.WriteLine("Name=" + Name);
				writer.WriteLine("Level=" + Level);
				writer.WriteLine("Difficulty=" + Difficulty);
				writer.WriteLine("Hardcore=" + Hardcore);
				writer.WriteLine("Money=" + Money);
				writer.WriteLine("FinalLevel=" + FinalLevel);
				writer.WriteLine("MaxMana=" + MaxMana);
				writer.WriteLine("Kills=" + Kills);
				writer.WriteLine("Deaths=" + Deaths);
				writer.WriteLine("CurrentMode=" + CurrentMode);
				writer.WriteLine("CurrentType=" + CurrentType);
				writer.WriteLine("CurrentMapType=" + CurrentMapType);
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
				writer.WriteLine("Health=" + Health);

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
			if (File.Exists(FileExplorer.Saves + SaveName + ".yaml"))
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
	}
}
