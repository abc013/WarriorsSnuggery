using System.Collections.Generic;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Trophies
{
	public static class TrophyManager
	{
		public static readonly Dictionary<string, Trophy> Trophies = new Dictionary<string, Trophy>();

		public static void Load(List<TextNode> nodes)
		{
			foreach (var node in nodes)
				Trophies.Add(node.Key, new Trophy(node.Children));
		}
	}
}
