using System.Collections.Generic;
using System.IO;
using System.Linq;
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
		public int Health;
		public int Mana;
		public int Kills;
		public int Deaths;
		public int MaxMana;

		public GameMode Mode;
		public GameType Type;
		public bool[] Shroud;

		public readonly Dictionary<string, bool> UnlockedSpells = new Dictionary<string, bool>();
		public readonly Dictionary<string, bool> UnlockedActors = new Dictionary<string, bool>();
		public readonly List<string> UnlockedTrophies = new List<string>();

		// Static Values
		public int FinalLevel;
		public int Difficulty;
		public int Seed;
		public bool Hardcore;

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

			Mode = save.Mode;
			Type = save.Type;
			Shroud = save.Shroud;

			foreach (var unlock in save.UnlockedSpells)
				UnlockedSpells.Add(unlock.Key, unlock.Value);
			foreach (var unlock in save.UnlockedActors)
				UnlockedActors.Add(unlock.Key, unlock.Value);
			UnlockedTrophies = save.UnlockedTrophies.ToList();

			FinalLevel = save.FinalLevel;
			Difficulty = save.Difficulty;
			MaxMana = save.MaxMana;
			Seed = save.Seed;
			Hardcore = save.Hardcore;
		}

		GameStatistics() { }

		public GameStatistics Copy()
		{
			return new GameStatistics(this);
		}

		public void Update(Game game)
		{
			Health = game.World.LocalPlayer.Health == null ? 1 : game.World.LocalPlayer.Health.HP;
		}

		public bool ActorAvailable(PlayablePartInfo playable)
		{
			return Program.IgnoreTech || playable.Unlocked || UnlockedActors.ContainsKey(playable.InternalName) && UnlockedActors[playable.InternalName];
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
			Mode = world.Game.Mode;
			Type = world.Game.Type;
			var shroud = "Shroud=";
			for (int x = 0; x < world.ShroudLayer.Size.X; x++)
			{
				for (int y = 0; y < world.ShroudLayer.Size.Y; y++)
				{
					// TODO: also save other shrouds
					shroud += world.ShroudLayer.ShroudRevealed(Objects.Actor.PlayerTeam, x, y).GetHashCode() + ",";
				}
			}
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
				writer.WriteLine(shroud.TrimEnd(','));
				writer.WriteLine("Seed=" + Seed);
				writer.WriteLine("Actor=" + Actor);
				writer.WriteLine("\tHealth=" + Health);
				writer.WriteLine("\tMana=" + Mana);
				writer.WriteLine("UnlockedSpells=");
				foreach (var unlock in UnlockedSpells)
					writer.WriteLine("\t" + unlock.Key + "=" + unlock.Value);
				writer.WriteLine("UnlockedActors=");
				foreach (var unlock in UnlockedActors)
					writer.WriteLine("\t" + unlock.Key + "=" + unlock.Value);
				writer.WriteLine("UnlockedTrophies=");
				foreach (var unlock in UnlockedTrophies)
					writer.WriteLine("\t" + unlock + "=");

				writer.Flush();
				writer.Close();
			}
			if (withMap)
				saveMap(world);
		}

		void saveMap(World world)
		{
			world.Map.SaveFile(FileExplorer.Saves + SaveName + "_map.yaml", SaveName + "_map");
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
					case "Shroud":
						statistic.Shroud = node.Convert<bool[]>();
						break;
					case "Seed":
						statistic.Seed = node.Convert<int>();
						break;
					case "Actor":
						statistic.Actor = node.Convert<string>();

						foreach (var node2 in node.Children)
						{
							switch (node2.Key)
							{
								case "Health":
									statistic.Health = node2.Convert<int>();
									break;
								case "Mana":
									statistic.Mana = node2.Convert<int>();
									break;
							}
						}
						break;
					case "UnlockedSpells":
						foreach (var node2 in node.Children)
							statistic.UnlockedSpells.Add(node2.Key, node2.Convert<bool>());

						break;
					case "UnlockedActors":
						foreach (var node2 in node.Children)
							statistic.UnlockedActors.Add(node2.Key, node2.Convert<bool>());

						break;
					case "UnlockedTrophies":
						foreach (var node2 in node.Children)
							statistic.UnlockedTrophies.Add(node2.Key);

						break;
				}
			}

			return statistic;
		}
	}
}
