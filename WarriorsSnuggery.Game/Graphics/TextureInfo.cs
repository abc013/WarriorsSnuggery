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
		readonly TextureType type;

		public readonly int Tick;

		public readonly int Width;
		public readonly int Height;

		public TextureInfo(string file) : this(file, TextureType.IMAGE, 0, 0, 0, true) { }

		public TextureInfo(string file, TextureType type, int tick, MPos bounds, bool searchFile = true, bool load = true) : this(file, type, tick, bounds.X, bounds.Y, searchFile, load) { }

		public TextureInfo(string file, TextureType type, int tick, int width, int height, bool searchFile = true, bool load = true)
		{
			if (searchFile)
				file = FileExplorer.FindIn(FileExplorer.Misc, file);

			this.file = file;

			this.type = type;
			Tick = tick;

			Width = width;
			Height = height;

			if (load)
			{
				if (this.type == TextureType.IMAGE)
					textures = SheetManager.AddTexture(file, out Width, out Height);
				else
					textures = SheetManager.AddSprite(file, width, height);
			}
		}

		public Texture[] GetTextures()
		{
			if (textures == null)
				throw new System.Exception($"Tried to fetch textures from unloaded TextureInfo ({file})");

			if (type == TextureType.RANDOM)
				return new[] { textures[Program.SharedRandom.Next(textures.Length)] };

			return textures;
		}
	}
}
