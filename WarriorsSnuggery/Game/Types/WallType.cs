/*
 * User: Andreas
 * Date: 02.08.2018
 * Time: 16:56
 */
using WarriorsSnuggery;

namespace WarriorsSnuggery
{
	public class WallType
	{
		public readonly int ID;

		readonly ITexture[] textures;

		public readonly bool Blocks;
		public readonly bool Destructable;
		public readonly int Height;

		public WallType(int id, string texture, bool blocks, bool destructable, int height)
		{
			ID = id;
			textures = new TextureInfo(texture, TextureType.ANIMATION, 10, 24, 48).GetTextures();
			if (textures.Length < 2)
				throw new YamlInvalidNodeException(string.Format("Texture '{0}' of Wall '{1}' has not enough textures!", texture, id));

			Blocks = blocks;
			Destructable = destructable;
			Height = height;
		}

		public ITexture GetTexture(bool horizontal)
		{
			var half = textures.Length / 2;
			var random = Program.SharedRandom.Next(half);

			return horizontal ? textures[half + random] : textures[random];
		}
	}

}