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
			var fields = TypeLoader.GetFields(this);

			foreach (var node in nodes)
			{
				switch (node.Key)
				{
					case nameof(Effects):
						Effects = new Effect[node.Children.Count];
						for (int i = 0; i < node.Children.Count; i++)
						{
							var node2 = node.Children[i];
							Effects[i] = new Effect(node2.Children);
						}

						break;
					default:
						TypeLoader.SetValue(this, fields, node);
						break;
				}
			}
			
			if (Effects != null && Effects.Length > 0)
				MaxDuration = Effects.Max(e => e.Duration);
		}
	}
}
