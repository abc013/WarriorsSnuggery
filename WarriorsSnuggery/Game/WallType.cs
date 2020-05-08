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

		public Texture GetTexture(bool horizontal, byte neighborState, byte nState)
		{
			var half = textures.Length / 2;

			if (ConsiderWallsNearby)
			{
				//if (nState << 4 == 0 && nState >> 4 == 0 || nState == 0)
				//	Console.WriteLine("normal " + Convert.ToString(nState, 2));
				//else if (nState << 4 == 0)
				//	Console.WriteLine("left " + Convert.ToString(nState, 2));
				//else if (nState >> 4 == 0)
				//	Console.WriteLine("right " + Convert.ToString(nState, 2));

				var add = 0;
				if (neighborState == 1)
					add = 1;
				else if (neighborState == 2)
					add = 2;
				var count = half / 3;

				var ran = Program.SharedRandom.Next(count);
				return horizontal ? textures[half + add * count + ran] : textures[add * count + ran];
			}

			var ran2 = Program.SharedRandom.Next(half);
			return horizontal ? textures[half + ran2] : textures[ran2];
		}

		public Texture GetDamagedTexture(bool horizontal, bool heavily, byte neighborState, byte nState)
		{
			var texture = heavily ? damagedTextures2 : damagedTextures1;

			var half = texture.Length / 2;

			if (ConsiderWallsNearby)
			{
				var add = 0;
				if (neighborState == 1)
					add = 1;
				else if (neighborState == 10)
					add = 2;
				var count = half / 3;

				var ran = Program.SharedRandom.Next(count);
				return horizontal ? textures[half + add * count + ran] : textures[add * count + ran];
			}

			var random = Program.SharedRandom.Next(half);
			return horizontal ? texture[half + random] : texture[random];
		}
	}

}