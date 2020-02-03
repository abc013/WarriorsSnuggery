using System.Collections.Generic;

namespace WarriorsSnuggery.Graphics
{
	public static class UITextureManager
	{
		static readonly Dictionary<TextureInfo, ITexture[]> textures = new Dictionary<TextureInfo, ITexture[]>();
		static readonly Dictionary<string, TextureInfo> infos = new Dictionary<string, TextureInfo>();

		public static void Add(string name, TextureInfo info)
		{
			infos.Add(name, info);
			textures.Add(info, SpriteManager.AddTexture(info));
		}

		public static ITexture[] Get(string name)
		{
			return textures[infos[name]];
		}

		public static void Dispose()
		{
			textures.Clear();
		}
	}
}
