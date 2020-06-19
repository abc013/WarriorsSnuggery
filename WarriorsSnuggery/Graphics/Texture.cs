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
		public readonly string File;

		public readonly TextureType Type;
		public readonly int Tick;

		public readonly int Width;
		public readonly int Height;

		public TextureInfo(string file, bool searchFile = true) : this(file, TextureType.IMAGE, 0, 0, 0, searchFile) { }

		public TextureInfo(string file, TextureType type, int tick, int width, int height, bool searchFile = true)
		{
			if (searchFile)
				File = FileExplorer.FindIn(FileExplorer.Misc, file);
			else
				File = file;

			Type = type;
			Tick = tick;

			Width = width;
			Height = height;
		}

		//if the type is random or image, it is certain that in this array there's only one texture.
		public Texture[] GetTextures()
		{
			switch (Type)
			{
				case TextureType.RANDOM:
					var random = Program.SharedRandom;
					var images = SpriteManager.GetTexture(this);

					return new[] { images[random.Next(images.Length)] };
				default:
					return SpriteManager.GetTexture(this);
			}
		}

		public override int GetHashCode()
		{
			return File.GetHashCode() ^ Width ^ Height;
		}
	}

	public class Texture
	{
		public readonly int SheetID;

		public readonly int X;
		public readonly int Y;
		public readonly int Width;
		public readonly int Height;

		public Texture(int x, int y, int width, int height, int sheetID)
		{
			X = x;
			Y = y;
			Width = width;
			Height = height;
			SheetID = sheetID;
		}
	}
}
