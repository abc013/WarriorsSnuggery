using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.Trophies
{
	public class Trophy
	{
		[Desc("Name of the Trophy.")]
		public readonly string Name;
		[Desc("Description, e.g. what was achieved to get it.")]
		public readonly string Description;
		[Desc("Image for display.")]
		public readonly TextureInfo Image;

		public Trophy(MiniTextNode[] nodes)
		{
			Loader.PartLoader.SetValues(this, nodes);

			if (Image != null)
				SpriteManager.AddTexture(Image);
		}
	}
}
