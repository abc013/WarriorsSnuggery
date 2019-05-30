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
			get { return textures[Program.SharedRandom.Next(textures.Length)]; }
		}
		[Desc("Random base texture.", "Use \"Image\" as rule.")] // TODO: make better
		readonly ITexture[] textures;

		public ITexture Texture_Edge
		{
			get { return textures_edge[Program.SharedRandom.Next(textures_edge.Length)]; }
		}
		[Desc("Edge of the tile.", "use \"Edge\" beneath the rule \"Image\" as rule.")]
		readonly ITexture[] textures_edge;
		public ITexture Texture_Edge2 // For vertical edges
		{
			get { return textures_edge2?[Program.SharedRandom.Next(textures_edge2.Length)]; }
		}
		[Desc("(possible) Vertical Edge of the tile.", "use \"Edge_Vertical\" beneath the rule \"Image\" as rule.")]
		readonly ITexture[] textures_edge2;

		public ITexture Texture_Corner
		{
			get { return textures_corner[Program.SharedRandom.Next(textures_corner.Length)]; }
		}
		[Desc("Corner of the tile.", "use \"Corner\" beneath the rule \"Image\" as rule.")]
		readonly ITexture[] textures_corner;

		[Desc("If not 1, this will modify the speed of the player.")]
		public readonly float SpeedModifier;
		[Desc("If yes, this tile can overlap other tiles.")]
		public readonly bool Overlaps;
		[Desc("Overlap height. The higher the value, the more tiles with smaller numbers will be overlapped.", "This rule is defined under Overlaps.")]
		public readonly int OverlapHeight;
		[Desc("If true, weapons will leave behind smudge on impact.")]
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