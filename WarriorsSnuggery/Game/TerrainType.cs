using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.Objects
{
	public class TerrainType
	{
		public readonly ushort ID;

		public ITexture Texture
		{
			get { return sprite[Program.SharedRandom.Next(sprite.Length)]; }
		}
		readonly ITexture[] sprite;

		[Desc("Random base texture.")]
		public readonly string Sprite;

		public ITexture Texture_Edge
		{
			get { return edgeSprite[Program.SharedRandom.Next(edgeSprite.Length)]; }
		}
		readonly ITexture[] edgeSprite;

		[Desc("Edge of the tile.")]
		public readonly string EdgeSprite;

		public ITexture Texture_Edge2 // For vertical edges
		{
			get { return verticalEdgeSprite?[Program.SharedRandom.Next(verticalEdgeSprite.Length)]; }
		}
		readonly ITexture[] verticalEdgeSprite;

		[Desc("(possible) Vertical Edge of the tile.")]
		public readonly string VerticalEdgeSprite;

		public ITexture Texture_Corner
		{
			get { return cornerSprite[Program.SharedRandom.Next(cornerSprite.Length)]; }
		}
		readonly ITexture[] cornerSprite;

		[Desc("Corner of the tile.")]
		public readonly string CornerSprite;

		[Desc("Speed modifier for actors.")]
		public readonly float Speed;

		public readonly bool Overlaps;
		[Desc("Overlap height. The higher the value, the more tiles with smaller numbers will be overlapped.")]
		public readonly int OverlapHeight;
		[Desc("If true, weapons will leave behind smudge on impact.")]
		public readonly bool SpawnSmudge = true;

		public TerrainType(ushort id, MiniTextNode[] nodes, bool documentation = false)
		{
			ID = id;

			Loader.PartLoader.SetValues(this, nodes);

			Overlaps = EdgeSprite != null;

			if (!documentation)
			{
				if ((Sprite == null || Sprite == string.Empty))
					throw new YamlMissingNodeException(ID.ToString(), "Image");

				sprite = TerrainSpriteManager.AddTexture(new TextureInfo(Sprite, TextureType.ANIMATION, 10, 24, 24));
				if (Overlaps)
				{
					if (EdgeSprite != null)
						edgeSprite = TerrainSpriteManager.AddTexture(new TextureInfo(EdgeSprite, TextureType.ANIMATION, 10, 24, 24));

					if (CornerSprite != null)
						cornerSprite = TerrainSpriteManager.AddTexture(new TextureInfo(CornerSprite, TextureType.ANIMATION, 10, 24, 24));

					if (VerticalEdgeSprite != null)
						verticalEdgeSprite = TerrainSpriteManager.AddTexture(new TextureInfo(VerticalEdgeSprite, TextureType.ANIMATION, 10, 24, 24));
				}
			}
		}
	}
}