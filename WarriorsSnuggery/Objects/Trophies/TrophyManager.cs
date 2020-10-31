using System.Collections.Generic;

namespace WarriorsSnuggery.Trophies
{
	public static class TrophyManager
	{
		public static readonly Dictionary<string, Trophy> Trophies = new Dictionary<string, Trophy>();

		public static void Load(string directory, string file)
		{
			var trophies = RuleReader.FromFile(directory, file);

			foreach (var trophy in trophies)
				Trophies.Add(trophy.Key, new Trophy(trophy.Children));
		}
	}
}
