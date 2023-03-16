using System.Collections.Generic;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Spells
{
	public static class EffectCache
	{
		public static readonly TypeDictionary<Effect> Types = new TypeDictionary<Effect>();

		public static void Load(List<TextNode> nodes)
		{
			foreach (var node in nodes)
				Types.Add(node.Key, new Effect(node.Children));
		}
	}
}
