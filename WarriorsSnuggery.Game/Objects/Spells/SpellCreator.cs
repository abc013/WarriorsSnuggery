using System.Collections.Generic;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Spells
{
	public static class SpellCreator
	{
		public static readonly TypeDictionary<Spell> Types = new TypeDictionary<Spell>();

		public static void Load(List<TextNode> nodes)
		{
			foreach (var node in nodes)
				Types.Add(node.Key, new Spell(node.Children));
		}
	}
}
