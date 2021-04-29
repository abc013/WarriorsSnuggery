using System.Collections.Generic;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Objects
{
	public class TerrainType
	{
		public readonly ushort ID;

		[Desc("Random base texture.")]
		public readonly string Sprite;

		readonly TextureInfo baseTextureInfo;
		public Texture Texture => baseTextureInfo.GetTextures()[0];

		[Desc("Edge of the tile.", "Vertical Edges are overwritten by VerticalEdgeSprite if given.")]
		public readonly string EdgeSprite;
		[Desc("Bounds of the edge texture.")]
		public readonly MPos EdgeSpriteBounds;

		readonly TextureInfo edgeTextureInfo;
		public Texture EdgeTexture => edgeTextureInfo.GetTextures()[0];
		public CPos EdgeOffset => textureOffset(EdgeSpriteBounds);

		[Desc("Vertical Edge of the tile.")]
		public readonly string VerticalEdgeSprite;
		[Desc("Bounds of the vertical edge texture.")]
		public readonly MPos VerticalEdgeSpriteBounds;

		readonly TextureInfo verticalTextureInfo;
		public Texture VerticalEdgeTexture => verticalTextureInfo?.GetTextures()[0];
		public CPos VerticalEdgeOffset => textureOffset(VerticalEdgeSpriteBounds);

		[Desc("Corner of the tile.")]
		public readonly string CornerSprite;
		[Desc("Bounds of the corner texture.")]
		public readonly MPos CornerSpriteBounds;

		readonly TextureInfo cornerTextureInfo;
		public Texture CornerTexture => cornerTextureInfo.GetTextures()[0];
		public CPos CornerOffset => textureOffset(CornerSpriteBounds);

		[Desc("Overlay to render over the terrain.")]
		public readonly TextureInfo Overlay;

		[Desc("Speed modifier for actors.")]
		public readonly float Speed;
		[Desc("Possible damage to actors being on this ground, used every 2 ticks.")]
		public readonly int Damage;

		public readonly bool Overlaps;
		[Desc("Overlap height. The higher the value, the more tiles with smaller numbers will be overlapped.")]
		public readonly int OverlapHeight;
		[Desc("If true, weapons will leave behind smudge on impact.")]
		public readonly bool SpawnSmudge = true;

		public TerrainType(ushort id, List<TextNode> nodes, bool documentation = false)
		{
			ID = id;

			TypeLoader.SetValues(this, nodes);

			Overlaps = EdgeSprite != null;

			if (documentation)
				return;
			
			if (Sprite == null || Sprite == string.Empty)
				throw new MissingNodeException(ID.ToString(), "Image");

			baseTextureInfo = new TextureInfo(Sprite, TextureType.RANDOM, 24, 24);
			if (Overlaps)
			{
				if (EdgeSprite != null)
					edgeTextureInfo = new TextureInfo(EdgeSprite, TextureType.RANDOM, EdgeSpriteBounds);

				if (CornerSprite != null)
					cornerTextureInfo = new TextureInfo(CornerSprite, TextureType.RANDOM, CornerSpriteBounds);

				if (VerticalEdgeSprite != null)
					verticalTextureInfo = new TextureInfo(VerticalEdgeSprite, TextureType.RANDOM, VerticalEdgeSpriteBounds);
			}
		}

		CPos textureOffset(MPos bounds)
		{
			return new CPos(512, 512, 0) - new CPos((int)((bounds.X % MasterRenderer.PixelSize) * MasterRenderer.PixelMultiplier * 512), (int)((bounds.Y % MasterRenderer.PixelSize) * MasterRenderer.PixelMultiplier * 512), 0);
		}
	}
}