namespace WarriorsSnuggery.Graphics
{
	public class Font
	{
		public float PixelMultiplier => MasterRenderer.PixelMultiplier / 1.5f;

		public int Gap => (int)(512 * Info.SpaceSize.Y * PixelMultiplier);

		public int Width => (int)(512 * Info.MaxSize.X * PixelMultiplier);

		public int Height => (int)(512 * Info.MaxSize.Y * PixelMultiplier);

		readonly Texture[] characters;
		public readonly FontInfo Info;

		public Font(FontInfo info)
		{
			Info = info;
			characters = SheetManager.AddFont(info);
		}

		public int GetWidth(char c)
		{
			if (!FontManager.Characters.Contains(c))
				c = FontManager.UnknownCharacter;

			var pixelSize = char.IsWhiteSpace(c) ? Info.SpaceSize.X / 2 + 1 : Info.CharSizes[FontManager.Characters.IndexOf(c)].X;

			return (int)(512 * pixelSize * PixelMultiplier);
		}

		public int GetWidth(string s)
		{
			var width = 0;

			foreach (var c in s)
				width += GetWidth(c);

			return width;
		}

		public Texture GetTexture(char c)
		{
			if (!FontManager.Characters.Contains(c))
				c = FontManager.UnknownCharacter;

			return characters[FontManager.Characters.IndexOf(c)];
		}
	}
}
