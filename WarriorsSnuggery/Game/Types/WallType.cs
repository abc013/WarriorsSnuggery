using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.Objects
{
	public class WallType
	{
		public readonly int ID;

		[Desc("Texture of the wall", "Use \"Image\" as rule.")]
		readonly ITexture[] textures;

		[Desc("If yes, this wall will block objects with physics.")]
		public readonly bool Blocks;
		[Desc("If yes, the wall can be destroyed. Unused.")]
		public readonly bool Destructable;
		[Desc("Height of the wall.")]
		public readonly int Height;

		public WallType(int id, string texture, bool blocks, bool destructable, int height)
		{
			ID = id;
			textures = new TextureInfo(texture, TextureType.ANIMATION, 0, 24, 48).GetTextures();
			if (textures.Length < 2)
				throw new YamlInvalidNodeException(string.Format("Texture '{0}' of Wall '{1}' has not enough textures!", texture, id));

			Blocks = blocks;
			Destructable = destructable;
			Height = height;

			SpriteManager.AddTexture(new TextureInfo(texture, TextureType.ANIMATION, 0, 24, 48));
		}

		public ITexture GetTexture(bool horizontal)
		{
			var half = textures.Length / 2;
			var random = Program.SharedRandom.Next(half);

			return horizontal ? textures[half + random] : textures[random];
		}
	}

}