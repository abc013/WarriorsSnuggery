using System.Collections.Generic;

namespace WarriorsSnuggery.Graphics
{
	public static class UISpriteManager
	{
		static readonly Dictionary<string, TextureInfo> infos = new Dictionary<string, TextureInfo>();

		public static void Add(string name, string file)
		{
			Add(name, new TextureInfo(file));
		}

		public static void Add(string name, TextureInfo info)
		{
			infos.Add(name, info);
		}

		public static Texture[] Get(string name)
		{
			return infos[name].GetTextures();
		}

		public static void Dispose()
		{
			infos.Clear();
		}
	}
}
