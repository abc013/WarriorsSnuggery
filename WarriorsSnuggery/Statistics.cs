using System;

namespace WarriorsSnuggery
{
	public class GameStatistics
	{
		// Changing Values
		public int Level;
		public int Money;
		public string Actor;
		public int Health;
		public int Mana;
		public int Kills;
		public int Deaths;
		// Static Values
		public int LevelToReach;
		public int Difficulty;
		public int MaxMana;

		public GameStatistics(SaveStatistics save)
		{
			Level = save.Level;
			Money = save.Money;
			Actor = save.Actor;
			Health = save.Health;
			Mana = save.Mana;
			Kills = save.Kills;
			Deaths = save.Deaths;

			LevelToReach = save.LevelToReach;
			Difficulty = save.Difficulty;
			MaxMana = save.MaxMana;
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
			Level = game.Stats.Level;
			Money = game.Stats.Money;
			Difficulty = game.Stats.Difficulty;
			Actor = game.Stats.Actor;
			Kills = game.Stats.Kills;
			Deaths = game.Stats.Deaths;
			MaxMana = game.Stats.MaxMana;
			Mana = game.Stats.Mana;
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
