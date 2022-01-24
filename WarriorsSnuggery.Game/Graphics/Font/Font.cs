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

		public (int width, int height) Measure(char c)
		{
			if (!FontManager.Characters.Contains(c))
				c = FontManager.UnknownCharacter;

			if (char.IsWhiteSpace(c))
				return (WidthGap, MaxHeight / 2);

			var pixelSize = Info.CharSizes[FontManager.Characters.IndexOf(c)];

			return ((int)(pixelSize.X * multiplier), (int)(pixelSize.Y * multiplier));
		}

		public (int width, int height) Measure(string s)
		{
			var maxWidth = 0;
			var width = 0;

			var height = MaxHeight / 2;

			foreach (var c in s)
			{
				if (c == '\n')
				{
					if (width > maxWidth)
						maxWidth = width;

					width = 0;
					height += MaxHeight / 2 + HeightGap / 2;
					continue;
				}

				width += Measure(c).width;
			}

			if (width > maxWidth)
				maxWidth = width;

			return (maxWidth, height);
		}

		public Texture GetTexture(char c)
		{
			if (!FontManager.Characters.Contains(c))
				c = FontManager.UnknownCharacter;

			return characters[FontManager.Characters.IndexOf(c)];
		}
	}
}
