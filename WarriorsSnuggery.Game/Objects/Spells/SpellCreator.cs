using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Spells
{
	public static class SpellCreator
	{
		public static readonly Dictionary<string, Spell> Types = new Dictionary<string, Spell>();

		public static void Load(List<TextNode> nodes)
		{
			foreach (var node in nodes)
				Types.Add(node.Key, new Spell(node.Children));
		}

		public static string GetName(Spell type)
		{
			return Types.First(t => t.Value == type).Key;
		}
	}
}
