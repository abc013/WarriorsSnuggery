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

		readonly Texture[] textures;

		public TextureInfo(string file) : this(file, TextureType.IMAGE, 0, 0, 0, true) { }

		public TextureInfo(string file, TextureType type, int tick, MPos bounds, bool searchFile = true, bool load = true) : this(file, type, tick, bounds.X, bounds.Y, searchFile, load) { }

		public TextureInfo(string file, TextureType type, int tick, int width, int height, bool searchFile = true, bool load = true)
		{
			if (searchFile)
				File = FileExplorer.FindIn(FileExplorer.Misc, file);
			else
				File = file;

			Type = type;
			Tick = tick;

			Width = width;
			Height = height;

			if (load)
				textures = SpriteManager.AddTexture(this);
		}

		public Texture[] GetTextures()
		{
			if (textures == null)
				throw new System.Exception($"Tried to fetch textures from unloaded TextureInfo ({File})");

			if (Type == TextureType.RANDOM)
				return new[] { textures[Program.SharedRandom.Next(textures.Length)] };

			return textures;
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
