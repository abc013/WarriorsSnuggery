using System.Collections.Generic;

namespace WarriorsSnuggery
{
	public static class GameSaveManager
	{
		public static GameStatistics DefaultStatistic;

		public static readonly List<GameStatistics> Statistics = new List<GameStatistics>();

		public static void Load()
		{
			foreach (var file in FileExplorer.FilesIn(FileExplorer.Saves))
			{
				if (!file.EndsWith("_map")) //make sure that we don't add any maps
					Statistics.Add(new GameStatistics(file));
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
			statistic.SetName(name);
			statistic.Save(game.World);

			Reload();
		}
	}
}
