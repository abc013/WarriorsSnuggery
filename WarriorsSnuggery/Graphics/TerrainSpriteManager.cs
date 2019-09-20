using System;
using System.Collections.Generic;

namespace WarriorsSnuggery.Graphics
{
	public class TerrainSpriteManager : SpriteManager
	{
		public static int SheetID;

		// ´byte 1,2: ID; 3: offset; 4: type;
		static readonly Dictionary<uint, ImageRenderable> sprites = new Dictionary<uint, ImageRenderable>();

		public static void CreateSheet(int size)
		{
			SheetID = TextureManager.CreateTexture(size, size);
		}

		public static void AddTexture(TextureInfo info, ushort ID, byte offset, byte type)
		{
			
		}

		public static void Dispose()
		{

		}
	}
}
