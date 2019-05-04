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

		public readonly Dictionary<string, bool> UnlockedNodes = new Dictionary<string, bool>();

		// Static Values
		public int FinalLevel;
		public int Difficulty;
		public int MaxMana;
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

			foreach (var unlock in save.UnlockedNodes)
				UnlockedNodes.Add(unlock.Key, unlock.Value);

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
		}

		public void Save(World world, bool withMap = true)
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
			var map = world.Map;
			using (var writer = new System.IO.StreamWriter(FileExplorer.Saves + SaveName + "_map.yaml", false))
			{
				writer.WriteLine("Name=" + Name);
				writer.WriteLine("Size=" + map.Size.X + "," + map.Size.Y);

				var terrain = "Terrain=";
				for (int y = 0; y < world.TerrainLayer.Size.Y; y++)
				{
					for (int x = 0; x < world.TerrainLayer.Size.X; x++)
					{
						terrain += world.TerrainLayer.Terrain[x, y].Type.ID + ",";
					}
				}

				terrain = terrain.Substring(0, terrain.Length - 1);
				writer.WriteLine(terrain);

				writer.WriteLine("Actors=");
				foreach (var a in world.Actors)
					writer.WriteLine("\t" + ActorCreator.GetName(a.Type) + ";" + a.Team + ";" + (a.IsBot + "").ToLower() + "=" + a.Position.X + "," + a.Position.Y + "," + a.Position.Z);

				var walls = "Walls=";
				for (int y = 0; y < world.WallLayer.Size.Y - 1; y++)
				{
					for (int x = 0; x < world.WallLayer.Size.X - 1; x++)
					{
						walls += (world.WallLayer.Walls[x, y] == null ? -1 : world.WallLayer.Walls[x, y].Type.ID) + ",";
					}
				}

				walls = walls.Substring(0, walls.Length - 1);
				writer.WriteLine(walls);

				writer.Flush();
				writer.Close();
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
					case "Unlocks":

						foreach(var node2 in node.Children)
						{
							statistic.UnlockedNodes.Add(node2.Key, node2.ToBoolean());
						}
						break;
				}
			}

			return statistic;
		}
	}
}
