using System;
using System.Drawing;
using System.Linq;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.Loader
{
	public static class FontLoader
	{
		public static float[][] LoadCharacters(FontInfo info)
		{
			var characters = new float[FontManager.Characters.Length][];
			var sizes = new MPos[characters.Length];

			using var measureBmp = new Bitmap(1, 1);
			using var measureGfx = System.Drawing.Graphics.FromImage(measureBmp);

			using (var font = new System.Drawing.Font(FontManager.Collection.Families.Where(a => a.Name == info.FontName).First(), info.Size, GraphicsUnit.Pixel))
			{
				var maxWidth = 0;
				var maxHeight = 0;
				for (int i = 0; i < FontManager.Characters.Length; i++)
				{
					var charBmp = generateFontChar(measureGfx, font, FontManager.Characters[i]);
					sizes[i] = new MPos(charBmp.Width, charBmp.Height);
					if (charBmp.Width > maxWidth)
						maxWidth = charBmp.Width;
					if (charBmp.Height > maxHeight)
						maxHeight = charBmp.Height;

					characters[i] = BitmapLoader.LoadTexture(charBmp, new Rectangle(Point.Empty, charBmp.Size));
				}

				info.SetSizes(new MPos(maxWidth, maxHeight), sizes);
			}

			return characters;
		}

		static Bitmap generateFontChar(System.Drawing.Graphics measureGfx, System.Drawing.Font font, char c)
		{
			var size = measureGfx.MeasureString(c.ToString(), font);
			var bmp = new Bitmap((int)Math.Ceiling(size.Width), (int)Math.Ceiling(size.Height));

			using (var gfx = System.Drawing.Graphics.FromImage(bmp))
			{
				gfx.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
				gfx.Clear(System.Drawing.Color.FromArgb(0));
				gfx.DrawString(c.ToString(), font, Brushes.White, 0, 0);
			}

			return bmp;
		}
	}
}
