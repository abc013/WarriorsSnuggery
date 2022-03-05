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

			var font = new SixLabors.Fonts.Font(FontManager.Collection.Get(info.FontName), info.Size);
			var renderOptions = new TextOptions(font);

			var brush = Brushes.Solid(SixLabors.ImageSharp.Color.White);

			var maxWidth = 0;
			var maxHeight = 0;
			for (int i = 0; i < FontManager.Characters.Length; i++)
			{
				var img = generateFontChar(font, brush, renderOptions, FontManager.Characters[i]);
				var span = new Span<RgbaVector>(new RgbaVector[img.Width * img.Height]);
				img.CopyPixelDataTo(span);

				sizes[i] = new MPos(img.Width, img.Height);
				if (img.Width > maxWidth)
					maxWidth = img.Width;
				if (img.Height > maxHeight)
					maxHeight = img.Height;

				characters[i] = BitmapLoader.LoadSelection(span, (img.Width, img.Height), (0, 0, img.Width, img.Height));
			}

			info.SetSizes(new MPos(maxWidth, maxHeight), sizes);

			return characters;
		}

		static Image<RgbaVector> generateFontChar(SixLabors.Fonts.Font font, IBrush brush, TextOptions textOptions, char c)
		{
			if (c == '/') // TODO: wtf
				c = '|';

			var size = TextMeasurer.Measure(c.ToString(), textOptions);
			var img = new Image<RgbaVector>((int)Math.Ceiling(size.Width), (int)Math.Ceiling(size.Height));
			var drawingOptions = new DrawingOptions()
			{
				GraphicsOptions = new GraphicsOptions()
				{
					Antialias = false
				}
			};

			img.Mutate(x => x.DrawText(drawingOptions, c.ToString(), font, brush, PointF.Empty));

			return img;
		}
	}
}
