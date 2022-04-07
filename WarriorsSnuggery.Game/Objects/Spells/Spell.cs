using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Spells
{
	public class Spell
	{
		public int MaxDuration;

		[Desc("Contains a list of all effects that the spell uses.")]
		public readonly Effect[] Effects;

		[Desc("Play sound when the spell is casted.")]
		public readonly SoundType Sound;

		public Spell(List<TextNode> nodes)
		{
			TypeLoader.SetValues(this, nodes);
			
			if (Effects != null && Effects.Length > 0)
				MaxDuration = Effects.Max(e => e.Duration);
		}
	}
}
