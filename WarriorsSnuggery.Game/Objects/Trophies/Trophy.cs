using System.Collections.Generic;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.Trophies
{
	[Desc("Trophies are little collectibles hidden in levels.")]
	public class Trophy
	{
		[Desc("Name of the Trophy.")]
		public readonly string Name;
		[Desc("Description, e.g. what was achieved to get it.")]
		public readonly string Description;
		[Desc("Image for display.")]
		public readonly TextureInfo Image;

		[Desc("Increases maximal mana value for the player.")]
		public readonly int MaxManaIncrease;
		[Desc("Gives a condition with the following name when collected.")]
		public readonly string ConditionName;

		public Trophy(List<MiniTextNode> nodes)
		{
			Loader.PartLoader.SetValues(this, nodes);

			if (Image != null)
				SpriteManager.AddTexture(Image);
		}
	}
}
