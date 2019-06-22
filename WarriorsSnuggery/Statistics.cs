/*
 * User: Andreas
 * Date: 19.10.2018
 * Time: 18:24
 */
using System.Collections.Generic;

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

		public GameMode Mode;

		public readonly Dictionary<string, bool> UnlockedNodes = new Dictionary<string, bool>();

		// Static Values
		public int FinalLevel;
		public int Difficulty;
		public int MaxMana;
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

			foreach (var unlock in save.UnlockedNodes)
				UnlockedNodes.Add(unlock.Key, unlock.Value);

			FinalLevel = save.FinalLevel;
			Difficulty = save.Difficulty;
			MaxMana = save.MaxMana;
			Seed = save.Seed;
			Hardcore = save.Hardcore;
		}

		GameStatistics()
		{

		}

		public GameStatistics Copy()
		{
			return new GameStatistics(this);
		}

		public void Update(Game game)
		{
			Health = game.World.LocalPlayer.Health == null ? 1 : game.World.LocalPlayer.Health.HP;
		}

		public void Save(World world, bool withMap = true)
		{
			Mode = world.Game.Mode;
			using (var writer = new System.IO.StreamWriter(FileExplorer.Saves + SaveName + ".yaml", false))
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
				writer.WriteLine("Seed=" + Seed);
				writer.WriteLine("Actor=" + Actor);
				writer.WriteLine("\tHealth=" + Health);
				writer.WriteLine("\tMana=" + Mana);
				writer.WriteLine("Unlocks=");
				foreach (var unlock in UnlockedNodes)
					writer.WriteLine("\t" + unlock.Key + "=" + unlock.Value);

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
			System.IO.File.Delete(FileExplorer.Saves + SaveName + ".yaml");

			if (System.IO.File.Exists(FileExplorer.Saves + SaveName + "_map.yaml"))
				System.IO.File.Delete(FileExplorer.Saves + SaveName + "_map.yaml");
		}

		public void SetName(string name)
		{
			Name = name;
			const string invalidChars = "#*+'?=!.:;,";
			foreach (var c in invalidChars)
			{
				name = name.Replace(c, '-');
			}
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
			const string invalidChars = "#*+'?=!.:;,";
			foreach(var c in invalidChars)
			{
				name = name.Replace(c, '-');
			}
			statistic.SaveName = name;
			
			statistic.FinalLevel = difficulty * 10;
			statistic.Money = 100 - difficulty * 20;
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

			foreach (var node in RuleReader.Read(FileExplorer.FindPath(FileExplorer.Saves, name, ".yaml"), name + ".yaml"))
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
					case "Unlocks":

						foreach(var node2 in node.Children)
						{
							statistic.UnlockedNodes.Add(node2.Key, node2.Convert<bool>());
						}
						break;
				}
			}

			return statistic;
		}
	}
}
