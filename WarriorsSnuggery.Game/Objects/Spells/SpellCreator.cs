using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Spells
{
	public static class SpellCreator
	{
		public static readonly Dictionary<string, Spell> Types = new Dictionary<string, Spell>();

		public static void Load(string directory, string file)
		{
			var spells = TextNodeLoader.FromFile(directory, file);

			foreach (var spell in spells)
				Types.Add(spell.Key, new Spell(spell.Children));
		}

		public static string GetName(Spell type)
		{
			return Types.First(t => t.Value == type).Key;
		}
	}
}
