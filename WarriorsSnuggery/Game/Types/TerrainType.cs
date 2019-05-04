using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery
{
	public class TerrainType
	{
		public readonly int ID;

		public ITexture Texture
		{
			get { return textures[Program.SharedRandom.Next(textures.Length)]; }
		}
		readonly ITexture[] textures;

		public ITexture Texture_Edge
		{
			get { return textures_edge[Program.SharedRandom.Next(textures_edge.Length)]; }
		}
		readonly ITexture[] textures_edge;
		public ITexture Texture_Edge2 // For vertical edges
		{
			get { return textures_edge2?[Program.SharedRandom.Next(textures_edge2.Length)]; }
		}
		readonly ITexture[] textures_edge2;

		public ITexture Texture_Corner
		{
			get { return textures_corner[Program.SharedRandom.Next(textures_corner.Length)]; }
		}
		readonly ITexture[] textures_corner;

		public readonly float SpeedModifier;
		public readonly bool Overlaps;
		public readonly int OverlapHeight;
		public readonly bool SpawnSmudge;

		public TerrainType(int id, string texture, float speedModifier, bool overlaps, bool spawnSmudge, int overlapHeight, string texture_edge, string texture_corner, string texture_edge2)
		{
			ID = id;
			textures = new TextureInfo(texture, TextureType.ANIMATION, 10, 24, 24).GetTextures();
			Overlaps = overlaps;
			OverlapHeight = overlapHeight;
			SpawnSmudge = spawnSmudge;
			if (overlaps)
			{
				textures_edge = new TextureInfo(texture_edge, TextureType.ANIMATION, 10, 24, 24).GetTextures();
				textures_corner = new TextureInfo(texture_corner, TextureType.ANIMATION, 10, 24, 24).GetTextures();
				if (texture_edge2 != "")
					textures_edge2 = new TextureInfo(texture_edge2, TextureType.ANIMATION, 10, 24, 24).GetTextures();
			}

			SpeedModifier = speedModifier;
		}
	}
}