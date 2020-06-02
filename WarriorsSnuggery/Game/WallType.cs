using System;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.Objects
{
	public class WallType
	{
		public readonly int ID;

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
		[Desc("Height of the wall.")]
		public readonly int Height = 1024;

		[Desc("Health of the wall.", "If 0 or negative, the wall is invincible.")]
		public readonly int Health = 0;
		public bool Invincible { get { return Health <= 0; } }

		[Desc("Spawns a specific wall when dying.")]
		public readonly int WallOnDeath = -1;

		[Desc("How much damage of nearby explosions penetrates the wall.")]
		public readonly float DamagePenetration = 0f;

		[Desc("Wall is only on floor and basically has no height.")]
		public readonly bool IsOnFloor;

		public WallType(int id, MiniTextNode[] nodes, bool documentation = false)
		{
			ID = id;
			Loader.PartLoader.SetValues(this, nodes);

			if (!documentation)
			{
				if (Image == null || string.IsNullOrEmpty(Image))
					throw new YamlMissingNodeException("[Wall] " + id, "Image");

				textures = SpriteManager.AddTexture(new TextureInfo(Image, TextureType.ANIMATION, 0, 24, 48));

				if (textures.Length < (ConsiderWallsNearby ? 6 : 2))
					throw new YamlInvalidNodeException(string.Format("Texture '{0}' of Wall '{1}' has not enough textures!", Image, id));

				if (DamagedImage1 != null)
				{
					damagedTextures1 = SpriteManager.AddTexture(new TextureInfo(DamagedImage1, TextureType.ANIMATION, 0, 24, 48));

					if (textures.Length < (ConsiderWallsNearby ? 6 : 2))
						throw new YamlInvalidNodeException(string.Format("DamageTexture '{0}' of Wall '{1}' has not enough textures!", Image, id));
				}

				if (DamagedImage2 != null)
				{
					damagedTextures2 = SpriteManager.AddTexture(new TextureInfo(DamagedImage2, TextureType.ANIMATION, 0, 24, 48));

					if (textures.Length < (ConsiderWallsNearby ? 6 : 2))
						throw new YamlInvalidNodeException(string.Format("DamageTexture '{0}' of Wall '{1}' has not enough textures!", Image, id));
				}
			}
		}

		public Texture GetTexture(bool horizontal, byte neighborState)
		{
			var half = textures.Length / 2;

			if (ConsiderWallsNearby)
			{
				var add = 0;
				var checks1 = (byte) (horizontal ? 0b00101011 : 0b00100011);
				var checks2 = (byte) (horizontal ? 0b10010100 : 0b01011000);

				//var state = Convert.ToString(neighborState, 2).PadLeft(8, '0');
				if (neighborState == 0 || (neighborState & checks1) != 0 && (neighborState & checks2) != 0)
				{
					//Console.WriteLine("normal " + state);
				}
				else if ((neighborState & checks2) == 0)
				{
					//Console.WriteLine("left " + state);
					add = 1;
				}
				else
				{
					//Console.WriteLine("right " + state);
					add = 2;
				}

				var count = half / 3;

				var ran = Program.SharedRandom.Next(count);
				return horizontal ? textures[half + add * count + ran] : textures[add * count + ran];
			}

			var ran2 = Program.SharedRandom.Next(half);
			return horizontal ? textures[half + ran2] : textures[ran2];
		}

		public Texture GetDamagedTexture(bool horizontal, bool heavily, byte neighborState)
		{
			var texture = heavily ? damagedTextures2 : damagedTextures1;

			var half = texture.Length / 2;

			if (ConsiderWallsNearby)
			{
				var add = 0;
				var checks1 = (byte)(horizontal ? 0b00101010 : 0b00100011);
				var checks2 = (byte)(horizontal ? 0b10010100 : 0b11000100);

				//var state = Convert.ToString(neighborState, 2).PadLeft(8, '0');
				if ((neighborState & checks1) != 0 && (neighborState & checks2) != 0 || neighborState == 0)
				{
					//Console.WriteLine("normal " + state);
				}
				else if ((neighborState & checks2) == 0)
				{
					//Console.WriteLine("left " + state);
					add = 1;
				}
				else
				{
					//Console.WriteLine("right " + state);
					add = 2;
				}

				var count = half / 3;

				var ran = Program.SharedRandom.Next(count);
				return horizontal ? texture[half + add * count + ran] : texture[add * count + ran];
			}

			var random = Program.SharedRandom.Next(half);
			return horizontal ? texture[half + random] : texture[random];
		}
	}

}