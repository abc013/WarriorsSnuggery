using System.Collections.Generic;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Trophies
{
	[Desc("Trophies are little collectibles hidden in levels.")]
	public class Trophy
	{
		[Require, Desc("Name of the Trophy.")]
		public readonly string Name;
		[Require, Desc("Description, e.g. what was achieved to get it.")]
		public readonly string Description;
		[Require, Desc("Image for display.")]
		public readonly TextureInfo Image;

		[Desc("Increases maximal mana value for the player.")]
		public readonly int MaxManaIncrease;
		[Desc("Increases the max lifes for the player.")]
		public readonly int MaxLifesIncrease;
		[Desc("Gives a condition with the following name when collected.")]
		public readonly string ConditionName;

		public Trophy(List<TextNode> nodes)
		{
			TypeLoader.SetValues(this, nodes);
		}
	}
}
