using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.Objects
{
	public class WallType
	{
		public readonly int ID;

		[Desc("Texture of the wall.")]
		public readonly string Image;
		readonly IImage[] textures;

		[Desc("Texture of the wall when slightly damaged.")]
		public readonly string DamagedImage1;
		readonly IImage[] damagedTextures1;

		[Desc("Texture of the wall when heavily damaged.")]
		public readonly string DamagedImage2;
		readonly IImage[] damagedTextures2;

		[Desc("If yes, this wall will block objects with physics.")]
		public readonly bool Blocks = true;
		[Desc("Height of the wall.")]
		public readonly int Height = 1024;

		[Desc("Health of the wall.", "If 0 or negative, the wall is invincible.")]
		public readonly int Health = 0;
		public bool Invincible { get { return Health <= 0; } }

		[Desc("How much damage of nearby explosions penetrates the wall.")]
		public readonly float DamagePenetration = 0f;

		public WallType(int id, MiniTextNode[] nodes, bool documentation = false)
		{
			ID = id;
			Loader.PartLoader.SetValues(this, nodes);

			if (!documentation)
			{
				if (Image == null || Image.Trim() == "")
					throw new YamlMissingNodeException("[Wall] " + id, "Image");

				textures = SpriteManager.AddTexture(new TextureInfo(Image, TextureType.ANIMATION, 0, 24, 48));

				if (textures.Length < 2)
					throw new YamlInvalidNodeException(string.Format("Texture '{0}' of Wall '{1}' has not enough textures!", Image, id));

				if (DamagedImage1 != null)
				{
					damagedTextures1 = SpriteManager.AddTexture(new TextureInfo(DamagedImage1, TextureType.ANIMATION, 0, 24, 48));

					if (textures.Length < 2)
						throw new YamlInvalidNodeException(string.Format("DamageTexture '{0}' of Wall '{1}' has not enough textures!", Image, id));
				}

				if (DamagedImage2 != null)
				{
					damagedTextures2 = SpriteManager.AddTexture(new TextureInfo(DamagedImage2, TextureType.ANIMATION, 0, 24, 48));

					if (textures.Length < 2)
						throw new YamlInvalidNodeException(string.Format("DamageTexture '{0}' of Wall '{1}' has not enough textures!", Image, id));
				}
			}
		}

		public IImage GetTexture(bool horizontal)
		{
			var half = textures.Length / 2;
			var random = Program.SharedRandom.Next(half);

			return horizontal ? textures[half + random] : textures[random];
		}

		public IImage GetDamagedTexture(bool horizontal, bool heavily)
		{
			var texture = heavily ? damagedTextures2 : damagedTextures1;

			var half = texture.Length / 2;
			var random = Program.SharedRandom.Next(half);

			return horizontal ? texture[half + random] : texture[random];
		}
	}

}