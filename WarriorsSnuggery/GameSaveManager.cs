using System;
using System.Collections.Generic;
using System.IO;

namespace WarriorsSnuggery
{
	public static class GameSaveManager
	{
		public static SaveStatistics CurrentStatistic;
		public static SaveStatistics DefaultStatistic;

		public static List<SaveStatistics> Statistics = new List<SaveStatistics>();

		public static void Load()
		{
			foreach (var file in Directory.GetFiles(FileExplorer.Saves))
			{
				Statistics.Add(new SaveStatistics(file.Remove(0,file.LastIndexOf('\\') + 1).Replace(".yaml", "")));
			}
		}

		public static void Reload()
		{
			Statistics.Clear();
			Load();
		}

		public static void Delete(SaveStatistics save)
		{
			save.Delete();
			Reload();
		}

		public static void NewStats(string name)
		{
			var stats = new SaveStatistics(name, true)
			{
				Name = name
			};
			stats.Save();
			Reload();
		}

		public static void Save(SaveStatistics save)
		{
			const string illegal = "\\\"\t?!;,|<>";
			foreach (var c in illegal)
				save.Name = save.Name.Replace(c.ToString(), "");
			save.Save();
		}
	}
}
