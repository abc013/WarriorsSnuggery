namespace WarriorsSnuggery.Graphics
{
	public class Font
	{
		public float PixelMultiplier
		{
			get { return MasterRenderer.PixelMultiplier / 1.5f; }
		}

		public int Gap
		{
			get { return (int)(10 * Info.SpaceSize.Y * PixelMultiplier); }
		}

		public int Width
		{
			get { return (int)(512 * Info.MaxSize.X * PixelMultiplier); }
		}

		public int Height
		{
			get { return (int)(1024 * Info.MaxSize.Y * PixelMultiplier); }
		}

		readonly Texture[] characters;
		public readonly FontInfo Info;

		public Font(FontInfo info)
		{
			Info = info;
			characters = SpriteManager.AddFont(info);
		}

		public int GetWidth(char c)
		{
			return Info.CharSizes[FontManager.Characters.IndexOf(c)].X;
		}

		public Texture GetTexture(char c)
		{
			return characters[FontManager.Characters.IndexOf(c)];
		}
	}
}
