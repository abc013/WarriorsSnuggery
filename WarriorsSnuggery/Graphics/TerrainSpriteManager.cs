using System;
using System.Collections.Generic;

namespace WarriorsSnuggery.Graphics
{
	public class TerrainSpriteManager
	{
		public static int TextureID;

		// ´byte 1,2: ID; 3: offset; 4: type;
		static readonly Dictionary<uint, ImageRenderable> sprites = new Dictionary<uint, ImageRenderable>();

		public static void AddTexture(TextureInfo info, ushort ID, byte offset, byte type)
		{
			
		}
	}
}
