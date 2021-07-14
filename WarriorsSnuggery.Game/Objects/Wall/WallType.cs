using System.Collections.Generic;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Physics;

namespace WarriorsSnuggery.Objects
{
	public class WallType
	{
		public readonly short ID;

		[Desc("Texture of the wall.")]
		public readonly TextureInfo Texture;

		[Desc("Texture of the wall when slightly damaged.")]
		public readonly TextureInfo SlightDamageTexture;

		[Desc("Texture of the wall when heavily damaged.")]
		public readonly TextureInfo HeavyDamageTexture;

		[Desc("This settings determines the texture of the wall by walls that are placed nearby.", "For this setting, three textures are needed in total: One for nearby walls only at left/top side, one for right/bottom side and a default one.", "This applies to all damage levels.")]
		public readonly bool ConsiderWallsNearby = false;
		[Desc("Set this to ignore it for the WallsNearby check.")]
		public readonly bool IgnoreForNearby = false;

		[Desc("If yes, this wall will block objects with physics.")]
		public readonly bool Blocks = true;
		[Desc("Height of the wall.", "This will allow players flying above this height to pass the wall and also see behind it.")]
		public readonly int Height = 1024;

		[Desc("Determines whether this walls blocks the visual terrain overlaps.")]
		public readonly bool BlocksTerrainOverlap = false;

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

		public readonly SimplePhysicsType HorizontalPhysicsType;
		public readonly SimplePhysicsType VerticalPhysicsType;

		public WallType(short id, List<TextNode> nodes, bool documentation = false)
		{
			ID = id;
			TypeLoader.SetValues(this, nodes);

			if (documentation)
				return;

			if (Texture == null)
				throw new MissingNodeException("[Wall] " + id, "Image");

			checkTextures(Texture);

			if (SlightDamageTexture != null)
				checkTextures(SlightDamageTexture);

			if (HeavyDamageTexture != null)
				checkTextures(HeavyDamageTexture);

			HorizontalPhysicsType = new SimplePhysicsType(Shape.LINE_HORIZONTAL, 512, 512, Height, new CPos(0, 0, 0), 0);
			VerticalPhysicsType = new SimplePhysicsType(Shape.LINE_VERTICAL, 512, 512, Height, new CPos(0, 512, 0), 0);
		}

		void checkTextures(TextureInfo info)
		{
			if (info.Type != TextureType.ANIMATION)
				throw new InvalidNodeException($"Texture '{info}' of Wall '{ID}' has to be defined as ANIMATION.");

			var textureCount = info.GetTextures().Length;
			var wallCount = ConsiderWallsNearby ? 6 : 2;
			if (textureCount < wallCount)
				throw new InvalidNodeException($"Texture '{info}' of Wall '{ID}' has not enough textures ({textureCount}/{wallCount})!");
		}

		public Texture GetTexture(bool horizontal, byte neighborState, TextureInfo info)
		{
			var usedTextures = info.GetTextures();

			var half = usedTextures.Length / 2;
			var start = horizontal ? half : 0;

			if (ConsiderWallsNearby)
			{
				var offset = getOffset(neighborState);
				var count = half / 3;

				var ran = Program.SharedRandom.Next(count);
				return usedTextures[start + offset * count + ran];
			}

			var halfRan = Program.SharedRandom.Next(half);
			return usedTextures[start + halfRan];
		}

		static int getOffset(byte neighborState)
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