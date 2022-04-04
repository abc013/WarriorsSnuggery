using System.Collections.Generic;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Graphics
{
	public static class UISpriteManager
	{
		static readonly Dictionary<string, TextureInfo> infos = new Dictionary<string, TextureInfo>();

		public static void Add(PackageFile file)
		{
			Add(file.ToString(), new TextureInfo(file));
		}

		public static void Add(string packageFile, TextureInfo info)
		{
			infos.Add(packageFile, info);
		}

		public static Texture[] Get(string packageFile)
		{
			return infos[packageFile].GetTextures();
		}

		public static void Dispose()
		{
			infos.Clear();
		}
	}
}
