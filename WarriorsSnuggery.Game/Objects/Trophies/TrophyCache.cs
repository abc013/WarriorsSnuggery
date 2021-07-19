using System.Collections.Generic;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Trophies
{
	public static class TrophyCache
	{
		public static readonly TypeDictionary<Trophy> Trophies = new TypeDictionary<Trophy>();

		public static void Load(List<TextNode> nodes)
		{
			foreach (var node in nodes)
				Trophies.Add(node.Key, new Trophy(node.Children));
		}
	}
}
