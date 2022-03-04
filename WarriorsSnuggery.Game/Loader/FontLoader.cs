using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using System;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.Loader
{
	public static class FontLoader
	{
		public static float[][] LoadCharacters(FontInfo info)
		{
			var characters = new float[FontManager.Characters.Length][];
			var sizes = new MPos[characters.Length];

			var font = new SixLabors.Fonts.Font(FontManager.Collection.Find(info.FontName), info.Size);
			var renderOptions = new RendererOptions(font);
			renderOptions.ApplyKerning = true;
			var brush = Brushes.Solid(SixLabors.ImageSharp.Color.White);

			var maxWidth = 0;
			var maxHeight = 0;
			for (int i = 0; i < FontManager.Characters.Length; i++)
			{
				var charImg = generateFontChar(font, brush, renderOptions, FontManager.Characters[i]);
				sizes[i] = new MPos(charImg.Width, charImg.Height);
				if (charImg.Width > maxWidth)
					maxWidth = charImg.Width;
				if (charImg.Height > maxHeight)
					maxHeight = charImg.Height;

				characters[i] = BitmapLoader.LoadSelection(charImg, (0, 0, charImg.Width, charImg.Height));
			}

			info.SetSizes(new MPos(maxWidth, maxHeight), sizes);

			return characters;
		}

		static Image<Rgba32> generateFontChar(SixLabors.Fonts.Font font, IBrush brush, RendererOptions renderOptions, char c)
		{
			var size = TextMeasurer.MeasureBounds(c.ToString(), renderOptions);
			var img = new Image<Rgba32>((int)Math.Ceiling(size.Width), (int)Math.Ceiling(size.Height));

			img.Mutate(x => x.DrawText(c.ToString(), font, brush, PointF.Empty));

			return img;
		}
	}
}
