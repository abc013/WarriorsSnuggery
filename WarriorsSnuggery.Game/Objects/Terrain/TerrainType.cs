using System.Collections.Generic;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Objects.Particles;

namespace WarriorsSnuggery.Objects
{
	public class TerrainType
	{
		public readonly ushort ID;

		[Require, Desc("Random base texture.")]
		public readonly PackageFile Sprite;

		readonly TextureInfo baseTextureInfo;
		public Texture Texture => baseTextureInfo.GetTextures()[0];

		[Desc("Edge of the tile.", "Vertical Edges are overwritten by VerticalEdgeSprite if given.")]
		public readonly PackageFile EdgeSprite;
		[Desc("Bounds of the edge texture.")]
		public readonly MPos EdgeSpriteBounds;

		readonly TextureInfo edgeTextureInfo;
		public Texture EdgeTexture => edgeTextureInfo.GetTextures()[0];
		public CPos EdgeOffset => textureOffset(EdgeSpriteBounds);

		[Desc("Vertical Edge of the tile.")]
		public readonly PackageFile VerticalEdgeSprite;
		[Desc("Bounds of the vertical edge texture.")]
		public readonly MPos VerticalEdgeSpriteBounds;

		readonly TextureInfo verticalTextureInfo;
		public Texture VerticalEdgeTexture => verticalTextureInfo?.GetTextures()[0];
		public CPos VerticalEdgeOffset => textureOffset(VerticalEdgeSpriteBounds);

		[Desc("Corner of the tile.")]
		public readonly PackageFile CornerSprite;
		[Desc("Bounds of the corner texture.")]
		public readonly MPos CornerSpriteBounds;

		readonly TextureInfo cornerTextureInfo;
		public Texture CornerTexture => cornerTextureInfo.GetTextures()[0];
		public CPos CornerOffset => textureOffset(CornerSpriteBounds);

		[Desc("Overlay to render over the terrain.")]
		public readonly TextureInfo Overlay;
		[Desc("Overlay to render over the terrain.")]
		public readonly bool UnifyOverlayTick = true;

		[Desc("Speed modifier for actors.")]
		public readonly float Speed;
		[Desc("Possible damage to actors being on this ground, used every 2 ticks.")]
		public readonly int Damage;

		public readonly bool Overlaps;
		[Desc("Overlap height. The higher the value, the more tiles with smaller numbers will be overlapped.")]
		public readonly int OverlapHeight;
		[Desc("If true, weapons will leave behind smudge on impact.")]
		public readonly bool SpawnSmudge = true;

		[Desc("Particles to spawn on this terrain.")]
		public readonly ParticleSpawner Particles;
		[Desc("Probability to spawn particles each tick.")]
		public readonly float ParticleProbability = 1f;

		public TerrainType(ushort id, List<TextNode> nodes, bool documentation = false)
		{
			ID = id;

			TypeLoader.SetValues(this, nodes);

			Overlaps = EdgeSprite != null;

			if (documentation)
				return;

			baseTextureInfo = new TextureInfo(Sprite, Constants.PixelSize, Constants.PixelSize, randomized: true);
			if (Overlaps)
			{
				if (EdgeSprite != null)
					edgeTextureInfo = new TextureInfo(EdgeSprite, EdgeSpriteBounds, randomized: true);

				if (CornerSprite != null)
					cornerTextureInfo = new TextureInfo(CornerSprite, CornerSpriteBounds, randomized: true);

				if (VerticalEdgeSprite != null)
					verticalTextureInfo = new TextureInfo(VerticalEdgeSprite, VerticalEdgeSpriteBounds, randomized: true);
			}
		}

		CPos textureOffset(MPos bounds)
		{
			return new CPos(512, 512, 0) - new CPos((int)((bounds.X % Constants.PixelSize) * Constants.PixelMultiplier * 512), (int)((bounds.Y % Constants.PixelSize) * Constants.PixelMultiplier * 512), 0);
		}
	}
}