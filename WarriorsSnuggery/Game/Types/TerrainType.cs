using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.Objects
{
	public class TerrainType
	{
		public readonly ushort ID;

		public IImage Texture
		{
			get { return Sprite[Program.SharedRandom.Next(Sprite.Length)]; }
		}
		[Desc("Random base texture.")]
		readonly IImage[] Sprite;

		public IImage Texture_Edge
		{
			get { return EdgeSprite[Program.SharedRandom.Next(EdgeSprite.Length)]; }
		}
		[Desc("Edge of the tile.")]
		readonly IImage[] EdgeSprite;
		public IImage Texture_Edge2 // For vertical edges
		{
			get { return VerticalEdgeSprite?[Program.SharedRandom.Next(VerticalEdgeSprite.Length)]; }
		}
		[Desc("(possible) Vertical Edge of the tile.")]
		readonly IImage[] VerticalEdgeSprite;

		public IImage Texture_Corner
		{
			get { return CornerSprite[Program.SharedRandom.Next(CornerSprite.Length)]; }
		}
		[Desc("Corner of the tile.")]
		readonly IImage[] CornerSprite;

		[Desc("If not 1, this will modify the speed of the player.")]
		public readonly float SpeedModifier;

		public readonly bool Overlaps;
		[Desc("Overlap height. The higher the value, the more tiles with smaller numbers will be overlapped.")]
		public readonly int OverlapHeight;
		[Desc("If true, weapons will leave behind smudge on impact.")]
		public readonly bool SpawnSmudge;

		public TerrainType(ushort id, string texture, float speedModifier, bool overlaps, bool spawnSmudge, int overlapHeight, string texture_edge, string texture_corner, string texture_edge2)
		{
			ID = id;
			Sprite = TerrainSpriteManager.AddTexture(new TextureInfo(texture, TextureType.ANIMATION, 10, 24, 24));
			Overlaps = overlaps;
			OverlapHeight = overlapHeight;
			SpawnSmudge = spawnSmudge;
			if (overlaps)
			{
				EdgeSprite = TerrainSpriteManager.AddTexture(new TextureInfo(texture_edge, TextureType.ANIMATION, 10, 24, 24));

				CornerSprite = TerrainSpriteManager.AddTexture(new TextureInfo(texture_corner, TextureType.ANIMATION, 10, 24, 24));

				if (texture_edge2 != "")
				{
					VerticalEdgeSprite = TerrainSpriteManager.AddTexture(new TextureInfo(texture_edge2, TextureType.ANIMATION, 10, 24, 24));
				}
			}

			SpeedModifier = speedModifier;
		}
	}
}