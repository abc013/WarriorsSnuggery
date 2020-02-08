using System.Collections.Generic;

namespace WarriorsSnuggery.Graphics
{
	public static class UITextureManager
	{
		static readonly Dictionary<TextureInfo, Texture[]> textures = new Dictionary<TextureInfo, Texture[]>();
		static readonly Dictionary<string, TextureInfo> infos = new Dictionary<string, TextureInfo>();

		public static void Add(string name, TextureInfo info)
		{
			infos.Add(name, info);
			textures.Add(info, SpriteManager.AddTexture(info));
		}

		public static Texture[] Get(string name)
		{
			return textures[infos[name]];
		}

		public static void Dispose()
		{
			textures.Clear();
			infos.Clear();
		}
	}
}
