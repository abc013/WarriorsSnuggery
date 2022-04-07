using System.Collections.Generic;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Spells
{
	public class Spell
	{
		[Require, Desc("Contains a list of all effects that the spell uses.")]
		public readonly Effect[] Effects;

		public Spell(List<TextNode> nodes)
		{
			TypeLoader.SetValues(this, nodes);
		}
	}
}
