using System.Collections.Generic;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Spells
{
	public static class SpellCasterCache
	{
		public static readonly List<SpellCasterType> Types = new List<SpellCasterType>();

		public static void Load(List<TextNode> nodes)
		{
			foreach (var node in nodes)
				Types.Add(new SpellCasterType(node.Children, node.Key));
		}
	}
}
