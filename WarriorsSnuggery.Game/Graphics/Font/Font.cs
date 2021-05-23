namespace WarriorsSnuggery.Graphics
{
	public class Font
	{
		const float multiplier = 512 * MasterRenderer.PixelMultiplier;

		public readonly FontInfo Info;
		readonly Texture[] characters;

		public readonly int WidthGap;
		public readonly int HeightGap;

		public readonly int MaxWidth;
		public readonly int MaxHeight;

		public Font(FontInfo info)
		{
			Info = info;
			characters = SheetManager.AddFont(info);

			WidthGap = (int)(Info.SpaceSize.X * multiplier);
			HeightGap = (int)(Info.SpaceSize.Y * multiplier);
			MaxWidth = (int)(Info.MaxSize.X * multiplier);
			MaxHeight = (int)(Info.MaxSize.Y * multiplier);
		}

		public int GetWidth(char c)
		{
			if (!FontManager.Characters.Contains(c))
				c = FontManager.UnknownCharacter;

			if (char.IsWhiteSpace(c))
				return WidthGap;

			var pixelSize = Info.CharSizes[FontManager.Characters.IndexOf(c)].X;

			return (int)(pixelSize * multiplier);
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
