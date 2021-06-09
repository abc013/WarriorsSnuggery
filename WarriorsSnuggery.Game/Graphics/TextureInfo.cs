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
		readonly string file;
		readonly Texture[] textures;

		public readonly TextureType Type;
		public readonly int Tick;

		public readonly int Width;
		public readonly int Height;

		public TextureInfo(string file) : this(file, TextureType.IMAGE, 0, 0, 0, true) { }

		public TextureInfo(string file, TextureType type, MPos bounds, int tick = 0, bool searchFile = true, bool load = true) : this(file, type, bounds.X, bounds.Y, tick, searchFile, load) { }

		public TextureInfo(string file, TextureType type, int width, int height, int tick = 0, bool searchFile = true, bool load = true)
		{
			if (searchFile)
				file = FileExplorer.FindIn(FileExplorer.Misc, file);

			this.file = file;

			Type = type;
			Tick = tick;

			Width = width;
			Height = height;

			if (load)
			{
				if (Type == TextureType.IMAGE)
					textures = SheetManager.AddTexture(file, out Width, out Height);
				else
					textures = SheetManager.AddSprite(file, width, height);
			}
		}

		public Texture[] GetTextures()
		{
			if (textures == null)
				throw new System.Exception($"Tried to fetch textures from unloaded TextureInfo ({file})");

			if (Type == TextureType.RANDOM)
				return new[] { textures[Program.SharedRandom.Next(textures.Length)] };

			return textures;
		}
	}
}
