using System.Collections.Generic;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Spells
{
	public static class SpellCasterCache
	{
		public static readonly Dictionary<string, SpellCasterType> Types = new Dictionary<string, SpellCasterType>();

		public static void Load(List<TextNode> nodes)
		{
			foreach (var node in nodes)
				Types.Add(node.Key, new SpellCasterType(node.Key, node.Children));
		}
	}
}
