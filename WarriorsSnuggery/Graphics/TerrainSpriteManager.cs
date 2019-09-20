using System;
using System.Collections.Generic;

namespace WarriorsSnuggery.Graphics
{
	public class TerrainSpriteManager : SpriteManager
	{
		public static Sheet sheet;

		// ´byte 1,2: ID; 3: offset; 4: type;
		static readonly Dictionary<uint, ImageRenderable> sprites = new Dictionary<uint, ImageRenderable>();

		public static void CreateSheet()
		{
			sheet = new Sheet(Settings.SheetSize);
			SheetBuilder.UseSheet(sheet);
		}

		public static void AddTexture(TextureInfo info, ushort ID, byte offset, byte type)
		{
			int w, h; // TODO remove
			var data = Loader.BitmapLoader.LoadTexture(info.File, out w, out h);
			// TODO use index
			SheetBuilder.WriteTexture(data, info);
		}

		public static void CreateTexture()
		{
			sheet.CreateTexture(true);
		}
	}
}
