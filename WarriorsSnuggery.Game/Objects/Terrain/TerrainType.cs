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

		public Texture[] Texture_Overlay { get; }

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

			Loader.TypeLoader.SetValues(this, nodes);

			Overlaps = EdgeSprite != null;

			if (documentation)
				return;
			
			if (Sprite == null || Sprite == string.Empty)
				throw new MissingNodeException(ID.ToString(), "Image");

			sprite = SpriteManager.AddTexture(new TextureInfo(Sprite, TextureType.ANIMATION, 10, 24, 24));
			if (Overlaps)
			{
				if (EdgeSprite != null)
					edgeSprite = SpriteManager.AddTexture(new TextureInfo(EdgeSprite, TextureType.ANIMATION, 10, EdgeSpriteBounds.X, EdgeSpriteBounds.Y));

				if (CornerSprite != null)
					cornerSprite = SpriteManager.AddTexture(new TextureInfo(CornerSprite, TextureType.ANIMATION, 10, CornerSpriteBounds.X, CornerSpriteBounds.Y));

				if (VerticalEdgeSprite != null)
					verticalEdgeSprite = SpriteManager.AddTexture(new TextureInfo(VerticalEdgeSprite, TextureType.ANIMATION, 10, VerticalEdgeSpriteBounds.X, VerticalEdgeSpriteBounds.Y));
			}

			if (Overlay != null)
				Texture_Overlay = SpriteManager.AddTexture(Overlay);
		}

		CPos textureOffset(MPos bounds)
		{
			return new CPos(512, 512, 0) - new CPos((int)((bounds.X % 24) * MasterRenderer.PixelMultiplier * 512), (int)((bounds.Y % 24) * MasterRenderer.PixelMultiplier * 512), 0);
		}
	}
}