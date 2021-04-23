using System.Collections.Generic;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Objects
{
	public class TerrainType
	{
		public readonly ushort ID;

		public Texture Texture => sprite[Program.SharedRandom.Next(sprite.Length)];
		readonly Texture[] sprite;

		[Desc("Random base texture.")]
		public readonly string Sprite;

		public Texture Texture_Edge => edgeSprite[Program.SharedRandom.Next(edgeSprite.Length)];
		readonly Texture[] edgeSprite;

		public CPos EdgeOffset => textureOffset(EdgeSpriteBounds);

		[Desc("Edge of the tile.")]
		public readonly string EdgeSprite;
		[Desc("Bounds of the edge texture.")]
		public readonly MPos EdgeSpriteBounds;

		// For vertical edges
		public Texture Texture_Edge2 => verticalEdgeSprite?[Program.SharedRandom.Next(verticalEdgeSprite.Length)];
		readonly Texture[] verticalEdgeSprite;

		public CPos VerticalEdgeOffset => textureOffset(VerticalEdgeSpriteBounds);

		[Desc("(possible) Vertical Edge of the tile.")]
		public readonly string VerticalEdgeSprite;
		[Desc("Bounds of the vertical edge texture.")]
		public readonly MPos VerticalEdgeSpriteBounds;

		public Texture Texture_Corner => cornerSprite[Program.SharedRandom.Next(cornerSprite.Length)];
		readonly Texture[] cornerSprite;

		public CPos CornerOffset => textureOffset(CornerSpriteBounds);

		[Desc("Corner of the tile.")]
		public readonly string CornerSprite;
		[Desc("Bounds of the corner texture.")]
		public readonly MPos CornerSpriteBounds;

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

			sprite = new TextureInfo(Sprite, TextureType.ANIMATION, 0, 24, 24).GetTextures();
			if (Overlaps)
			{
				if (EdgeSprite != null)
					edgeSprite = new TextureInfo(EdgeSprite, TextureType.ANIMATION, 10, EdgeSpriteBounds).GetTextures();

				if (CornerSprite != null)
					cornerSprite = new TextureInfo(CornerSprite, TextureType.ANIMATION, 10, CornerSpriteBounds).GetTextures();

				if (VerticalEdgeSprite != null)
					verticalEdgeSprite = new TextureInfo(VerticalEdgeSprite, TextureType.ANIMATION, 10, VerticalEdgeSpriteBounds).GetTextures();
			}
		}

		CPos textureOffset(MPos bounds)
		{
			return new CPos(512, 512, 0) - new CPos((int)((bounds.X % MasterRenderer.PixelSize) * MasterRenderer.PixelMultiplier * 512), (int)((bounds.Y % MasterRenderer.PixelSize) * MasterRenderer.PixelMultiplier * 512), 0);
		}
	}
}