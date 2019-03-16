/*
 * User: Andreas
 * Date: 19.10.2018
 * Time: 18:24
 */
using System;
using System.Linq;

namespace WarriorsSnuggery
{
	public class GameStatistics
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
		// Static Values
		public int FinalLevel;
		public int Difficulty;
		public int MaxMana;
		public bool Hardcore;

		public GameStatistics(SaveStatistics save)
		{
			Level = save.Level;
			Money = save.Money;
			Actor = save.Actor;
			Health = save.Health;
			Mana = save.Mana;
			Kills = save.Kills;
			Deaths = save.Deaths;

			FinalLevel = save.LevelToReach;
			Difficulty = save.Difficulty;
			MaxMana = save.MaxMana;
		}

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

			FinalLevel = save.FinalLevel;
			Difficulty = save.Difficulty;
			MaxMana = save.MaxMana;
			Hardcore = save.Hardcore;
		}

		public GameStatistics()
		{

		}

		public GameStatistics Copy()
		{
			return new GameStatistics(this);
		}

		public void Update(Game game)
		{
			Health = game.World.LocalPlayer.Health == null ? 1 : game.World.LocalPlayer.Health.HP;
			//Mana = game.World.LocalPlayer. TODO
		}

		public void Save()
		{
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
				writer.WriteLine("Actor=" + Actor);
				writer.WriteLine("\tHealth=" + Health);
				writer.WriteLine("\tMana=" + Mana);
			}
		}

		public void Delete()
		{
			System.IO.File.Delete(FileExplorer.Saves + SaveName + ".yaml");
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

		public static GameStatistics CreateGameStatistic(int difficulty, bool hardcore, string name)
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

			return statistic;
		}

		public static GameStatistics LoadGameStatistic(string name)
		{
			var statistic = new GameStatistics
			{
				SaveName = name
			};

			foreach (var node in RuleReader.Read(FileExplorer.FindIn(FileExplorer.Saves, name, ".yaml")))
			{
				switch (node.Key)
				{
					case "Name":
						statistic.Name = node.Value;
						break;
					case "Level":
						statistic.Level = node.ToInt();
						break;
					case "Difficulty":
						statistic.Difficulty = node.ToInt();
						break;
					case "Money":
						statistic.Money = node.ToInt();
						break;
					case "LevelAim":
						statistic.FinalLevel = node.ToInt();
						break;
					case "MaxMana":
						statistic.MaxMana = node.ToInt();
						break;
					case "Kills":
						statistic.Kills = node.ToInt();
						break;
					case "Deaths":
						statistic.Deaths = node.ToInt();
						break;
					case "Actor":
						statistic.Actor = node.Value;

						foreach (var node2 in node.Children)
						{
							switch (node2.Key)
							{
								case "Health":
									statistic.Health = node2.ToInt();
									break;
								case "Mana":
									statistic.Mana = node2.ToInt();
									break;
							}
						}
						break;
				}
			}

			return statistic;
		}
	}

	public class SaveStatistics
	{
		readonly string saveName;
		public string Name;
		public int Level;
		public int Money;
		public string Actor;
		public int Health;
		public int Mana;
		public int Kills;
		public int Deaths;

		public int LevelToReach;
		public int Difficulty;
		public int MaxMana;

		public SaveStatistics(string saveName, bool @new = false)
		{
			this.saveName = saveName;
			if (@new)
				return;
			foreach(var node in RuleReader.Read(FileExplorer.FindIn(FileExplorer.Saves, saveName, ".yaml")))
			{
				switch(node.Key)
				{
					case "Name":
						Name = node.Value;
						break;
					case "Level":
						Level = node.ToInt();
						break;
					case "Difficulty":
						Difficulty = node.ToInt();
						break;
					case "Money":
						Money = node.ToInt();
						break;
					case "LevelAim":
						LevelToReach = node.ToInt();
						break;
					case "MaxMana":
						MaxMana = node.ToInt();
						break;
					case "Kills":
						Kills = node.ToInt();
						break;
					case "Deaths":
						Deaths = node.ToInt();
						break;
					case "Actor":
						Actor = node.Value;

						foreach(var node2 in node.Children)
						{
							switch(node2.Key)
							{
								case "Health":
									Health = node2.ToInt();
									break;
								case "Mana":
									Mana = node2.ToInt();
									break;
							}
						}
						break;
				}
			}
		}

		public void SetValues(Game game)
		{
			Level = game.Statistics.Level;
			Money = game.Statistics.Money;
			Difficulty = game.Statistics.Difficulty;
			Actor = game.Statistics.Actor;
			Kills = game.Statistics.Kills;
			Deaths = game.Statistics.Deaths;
			MaxMana = game.Statistics.MaxMana;
			Mana = game.Statistics.Mana;
			if (game.World.LocalPlayer != null)
			{
				Health = game.World.LocalPlayer.Health.HP;
			}
		}

		public void Save()
		{
			using (var writer = new System.IO.StreamWriter(FileExplorer.Saves + saveName + ".yaml", false))
			{
				writer.WriteLine("Name=" + Name);
				writer.WriteLine("Level=" + Level);
				writer.WriteLine("Difficulty=" + Difficulty);
				writer.WriteLine("Money=" + Money);
				writer.WriteLine("LevelAim=" + LevelToReach);
				writer.WriteLine("MaxMana=" + MaxMana);
				writer.WriteLine("Kills=" + Kills);
				writer.WriteLine("Deaths=" + Deaths);
				writer.WriteLine("Actor=" + Actor);
				writer.WriteLine("\tHealth=" + Health);
				writer.WriteLine("\tMana=" + Mana);
			}
		}

		public void Delete()
		{
			System.IO.File.Delete(FileExplorer.Saves + saveName + ".yaml");
		}
	}
}
