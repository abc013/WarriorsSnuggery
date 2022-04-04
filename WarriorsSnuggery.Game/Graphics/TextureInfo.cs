using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Graphics
{
	public enum TextureType
	{
		IMAGE,
		ANIMATION,
		RANDOM
	}

	public sealed class TextureInfo
	{
		readonly string filepath;
		readonly Texture[] textures;

		public readonly TextureType Type;
		public readonly int Tick;

		public readonly int Width;
		public readonly int Height;

		public TextureInfo(PackageFile packageFile) : this(packageFile, TextureType.IMAGE, 0, 0, 0) { }

		public TextureInfo(PackageFile packageFile, TextureType type, MPos bounds, int tick = 0, bool load = true) : this(packageFile, type, bounds.X, bounds.Y, tick, load) { }

		public TextureInfo(PackageFile packageFile, TextureType type, int width, int height, int tick = 0, bool load = true)
		{
			filepath = FileExplorer.FindIn(packageFile.Package.ContentDirectory, packageFile.File, ".png");

			Type = type;
			Tick = tick;

			Width = width;
			Height = height;

			if (load)
			{
				if (Type == TextureType.IMAGE)
					textures = SheetManager.AddTexture(filepath, out Width, out Height);
				else
					textures = SheetManager.AddSprite(filepath, width, height);
			}
		}

		public Texture[] GetTextures()
		{
			if (textures == null)
				throw new System.Exception($"Tried to fetch textures from unloaded TextureInfo ({filepath}).");

			if (Type == TextureType.RANDOM)
				return new[] { textures[Program.SharedRandom.Next(textures.Length)] };

			return textures;
		}
	}
}
