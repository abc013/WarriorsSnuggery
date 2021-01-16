using System.Collections.Generic;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.Objects
{
	public class WallType
	{
		public readonly short ID;

		[Desc("Texture of the wall.")]
		public readonly string Image;
		readonly Texture[] textures;

		[Desc("Texture of the wall when slightly damaged.")]
		public readonly string DamagedImage1;
		readonly Texture[] damagedTextures1;

		[Desc("Texture of the wall when heavily damaged.")]
		public readonly string DamagedImage2;
		readonly Texture[] damagedTextures2;

		[Desc("This settings determines the texture of the wall by walls that are placed nearby.", "For this setting, three textures are needed in total: One for nearby walls only at left/top side, one for right/bottom side and a default one.", "This applies to all damage levels.")]
		public readonly bool ConsiderWallsNearby = false;
		[Desc("Set this to ignore it for the WallsNearby check.")]
		public readonly bool IgnoreForNearby = false;

		[Desc("If yes, this wall will block objects with physics.")]
		public readonly bool Blocks = true;
		[Desc("Height of the wall.", "This will allow players flying above this height to pass the wall and also see behind it.")]
		public readonly int Height = 1024;

		[Desc("Health of the wall.", "If 0 or negative, the wall is invincible.")]
		public readonly int Health = 0;
		public bool Invincible => Health <= 0;

		[Desc("Spawns a specific wall when dying.")]
		public readonly short WallOnDeath = -1;

		[Desc("How much damage of nearby explosions penetrates the wall.")]
		public readonly float DamagePenetration = 0f;

		[Desc("Wall is only on floor and basically has no height.")]
		public readonly bool IsOnFloor;

		[Desc("Wall is transparent.", "This is used for checking whether the player can see through this wall.")]
		public readonly bool IsTransparent;

		[Desc("Specifies the armor that the wall has.")]
		public readonly string Armor;

		public WallType(short id, List<MiniTextNode> nodes, bool documentation = false)
		{
			ID = id;
			Loader.PartLoader.SetValues(this, nodes);

			if (!documentation)
			{
				if (Image == null || string.IsNullOrEmpty(Image))
					throw new MissingNodeException("[Wall] " + id, "Image");

				textures = SpriteManager.AddTexture(new TextureInfo(Image, TextureType.ANIMATION, 0, 24, 48));

				if (textures.Length < (ConsiderWallsNearby ? 6 : 2))
					throw new InvalidTextNodeException(string.Format("Texture '{0}' of Wall '{1}' has not enough textures!", Image, id));

				if (DamagedImage1 != null)
				{
					damagedTextures1 = SpriteManager.AddTexture(new TextureInfo(DamagedImage1, TextureType.ANIMATION, 0, 24, 48));

					if (textures.Length < (ConsiderWallsNearby ? 6 : 2))
						throw new InvalidTextNodeException(string.Format("DamageTexture '{0}' of Wall '{1}' has not enough textures!", Image, id));
				}

				if (DamagedImage2 != null)
				{
					damagedTextures2 = SpriteManager.AddTexture(new TextureInfo(DamagedImage2, TextureType.ANIMATION, 0, 24, 48));

					if (textures.Length < (ConsiderWallsNearby ? 6 : 2))
						throw new InvalidTextNodeException(string.Format("DamageTexture '{0}' of Wall '{1}' has not enough textures!", Image, id));
				}
			}
		}

		public Texture GetTexture(bool horizontal, byte neighborState)
		{
			return getTexture(horizontal, neighborState, textures);
		}

		public Texture GetDamagedTexture(bool horizontal, bool heavily, byte neighborState)
		{
			return getTexture(horizontal, neighborState, heavily ? damagedTextures2 : damagedTextures1);
		}

		Texture getTexture(bool horizontal, byte neighborState, Texture[] usedTextures)
		{
			var half = usedTextures.Length / 2;
			var offset = horizontal ? half : 0;

			if (ConsiderWallsNearby)
			{
				var add = getNeighborState(neighborState);
				var count = half / 3;

				var ran = Program.SharedRandom.Next(count);
				return usedTextures[offset + add * count + ran];
			}

			var halfRan = Program.SharedRandom.Next(half);
			return usedTextures[offset + halfRan];
		}

		int getNeighborState(byte neighborState)
		{
			const byte checks1 = 0b11100000;
			const byte checks2 = 0b00011100;

			if (neighborState == 0 || (neighborState & checks1) != 0 && (neighborState & checks2) != 0)
				return 0;

			if ((neighborState & checks2) == 0)
				return 1;

			return 2;
		}
	}

}