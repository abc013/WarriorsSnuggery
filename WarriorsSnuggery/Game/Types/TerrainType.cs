/*
 * User: Andreas
 * Date: 02.08.2018
 * Time: 16:55
 */
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects.Parts;

namespace WarriorsSnuggery.Objects
{
	public class TerrainType
	{
		public readonly int ID;

		public ITexture Texture
		{
			get { return Sprite[Program.SharedRandom.Next(Sprite.Length)]; }
		}
		[Desc("Random base texture.")] // TODO: make better
		readonly ITexture[] Sprite;

		public ITexture Texture_Edge
		{
			get { return EdgeSprite[Program.SharedRandom.Next(EdgeSprite.Length)]; }
		}
		[Desc("Edge of the tile.")]
		readonly ITexture[] EdgeSprite;
		public ITexture Texture_Edge2 // For vertical edges
		{
			get { return VerticalEdgeSprite?[Program.SharedRandom.Next(VerticalEdgeSprite.Length)]; }
		}
		[Desc("(possible) Vertical Edge of the tile.")]
		readonly ITexture[] VerticalEdgeSprite;

		public ITexture Texture_Corner
		{
			get { return CornerSprite[Program.SharedRandom.Next(CornerSprite.Length)]; }
		}
		[Desc("Corner of the tile.")]
		readonly ITexture[] CornerSprite;

		[Desc("If not 1, this will modify the speed of the player.")]
		public readonly float SpeedModifier;

		public readonly bool Overlaps;
		[Desc("Overlap height. The higher the value, the more tiles with smaller numbers will be overlapped.")]
		public readonly int OverlapHeight;
		[Desc("If true, weapons will leave behind smudge on impact.")]
		public readonly bool SpawnSmudge;

		public TerrainType(int id, string texture, float speedModifier, bool overlaps, bool spawnSmudge, int overlapHeight, string texture_edge, string texture_corner, string texture_edge2)
		{
			ID = id;
			Sprite = new TextureInfo(texture, TextureType.ANIMATION, 10, 24, 24).GetTextures();
			Overlaps = overlaps;
			OverlapHeight = overlapHeight;
			SpawnSmudge = spawnSmudge;
			if (overlaps)
			{
				EdgeSprite = new TextureInfo(texture_edge, TextureType.ANIMATION, 10, 24, 24).GetTextures();
				CornerSprite = new TextureInfo(texture_corner, TextureType.ANIMATION, 10, 24, 24).GetTextures();
				if (texture_edge2 != "")
					VerticalEdgeSprite = new TextureInfo(texture_edge2, TextureType.ANIMATION, 10, 24, 24).GetTextures();
			}

			SpeedModifier = speedModifier;
		}
	}
}