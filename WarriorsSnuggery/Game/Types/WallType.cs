using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.Objects
{
	public class WallType
	{
		public readonly int ID;

		[Desc("Texture of the wall.")]
		public readonly string Image;

		readonly IImage[] textures;

		[Desc("If yes, this wall will block objects with physics.")]
		public readonly bool Blocks = true;
		[Desc("Height of the wall.")]
		public readonly int Height = 1024;

		[Desc("Health of the wall.")]
		public readonly int Health = -1;

		public WallType(int id, MiniTextNode[] nodes)
		{
			ID = id;
			Loader.PartLoader.SetValues(this, nodes);

			if (id >= 0)
			{
				if (Image == null)
					throw new YamlMissingNodeException("[Wall] " + id, "Image");

				textures = SpriteManager.AddTexture(new TextureInfo(Image, TextureType.ANIMATION, 0, 24, 48));

				if (textures.Length < 2)
					throw new YamlInvalidNodeException(string.Format("Texture '{0}' of Wall '{1}' has not enough textures!", Image, id));
			}
		}

		public IImage GetTexture(bool horizontal)
		{
			var half = textures.Length / 2;
			var random = Program.SharedRandom.Next(half);

			return horizontal ? textures[half + random] : textures[random];
		}
	}

}