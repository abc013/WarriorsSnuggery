using System.Collections.Generic;
using System.IO;

namespace WarriorsSnuggery
{
	public static class GameSaveManager
	{
		public static GameStatistics DefaultStatistic;

		public static List<GameStatistics> Statistics = new List<GameStatistics>();

		public static void Load()
		{
			foreach (var file in Directory.GetFiles(FileExplorer.Saves))
			{
				Statistics.Add(GameStatistics.LoadGameStatistic(file.Remove(0,file.LastIndexOf('\\') + 1).Replace(".yaml", "")));
			}
		}

		public static void Reload()
		{
			Statistics.Clear();
			Load();
		}

		public static void Delete(GameStatistics save)
		{
			save.Delete();
			Reload();
		}

		public static void SaveOnNewName(GameStatistics statistic, string name, Game game)
		{
			statistic.Update(game);
			var stats = statistic.Copy();

			stats.SetName(name);
			stats.Save(game.World);

			Reload();
		}

		public static void Save(GameStatistics save, Game game)
		{
			save.Update(game);
			save.Save(game.World);
		}
	}
}
