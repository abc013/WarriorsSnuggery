using System.Collections.Generic;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Spells
{
	public class Spell
	{
		[Require, Desc("Effect to use.")]
		public readonly Effect Effect;

		public Spell(List<TextNode> nodes)
		{
			TypeLoader.SetValues(this, nodes);
		}
	}
}
