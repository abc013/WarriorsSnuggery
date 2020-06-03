using OpenToolkit.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace WarriorsSnuggery.Graphics
{
	public static class TextureManager
	{
		public static int TextureCount;

		public static int Create(MPos size)
		{
			lock (MasterRenderer.GLLock)
			{
				int id = GL.GenTexture();
				TextureCount++;

				var emptyData = new float[size.X * size.Y * 4];
				emptyData.Initialize();

				Write(id, emptyData, size);

				Program.CheckGraphicsError("createTexture");

				return id;
			}
		}

		public static void Write(int id, float[] data, MPos size)
		{
			lock (MasterRenderer.GLLock)
			{
				GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
				GL.ActiveTexture(TextureUnit.Texture0);
				GL.BindTexture(TextureTarget.Texture2D, id);
				GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, size.X, size.Y, 0, PixelFormat.Rgba, PixelType.Float, data);

				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Nearest);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Nearest);

				Program.CheckGraphicsError("writeTexture");
			}
		}

		public static void Dispose(int id)
		{
			lock (MasterRenderer.GLLock)
			{
				GL.DeleteTexture(id);

				Program.CheckGraphicsError("disposeTexture");
			}
		}

		static Bitmap measureBmp;
		public static float[][] LoadCharacters(FontInfo info)
		{
			var characters = new float[FontManager.Characters.Length][];
			var sizes = new MPos[characters.Length];

			// If bugs start with font spacing, increase number of pixels here
			measureBmp = new Bitmap(64, 64);

			using (var font = new System.Drawing.Font(FontManager.Collection.Families.Where(a => a.Name == info.FontName).First(), info.Size, GraphicsUnit.Pixel))
			{
				var maxWidth = 0;
				var maxHeight = 0;
				for (int i = 0; i < FontManager.Characters.Length; i++)
				{
					var charBmp = generateFontChar(font, FontManager.Characters[i]);
					sizes[i] = new MPos(charBmp.Width, charBmp.Height);
					if (charBmp.Width > maxWidth)
						maxWidth = charBmp.Width;
					if (charBmp.Height > maxHeight)
						maxHeight = charBmp.Height;

					characters[i] = loadTexture(charBmp);
				}
				info.SetSizes(new MPos(maxWidth, maxHeight), sizes);
			}

			measureBmp.Dispose();
			return characters;
		}

		static Bitmap generateFontChar(System.Drawing.Font font, char c)
		{
			var size = getFontSize(font, c);
			var bmp = new Bitmap((int)Math.Ceiling(size.Width), (int)Math.Ceiling(size.Height));

			using (var gfx = System.Drawing.Graphics.FromImage(bmp))
			{
				gfx.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
				gfx.Clear(System.Drawing.Color.FromArgb(0));
				gfx.DrawString(c.ToString(), font, Brushes.White, 0, 0);
			}
			return bmp;
		}

		static SizeF getFontSize(System.Drawing.Font font, char c)
		{
			using var gfx = System.Drawing.Graphics.FromImage(measureBmp);

			return gfx.MeasureString(c.ToString(), font);
		}

		static float[] loadTexture(Bitmap bmp)
		{
			return Loader.BitmapLoader.LoadTexture(bmp, new Rectangle(Point.Empty, bmp.Size));
		}

		public static float[] LoadTexture(string filename, out int width, out int height)
		{
			if (!File.Exists(filename))
				throw new FileNotFoundException("The file `" + filename + "` has not been found.");

			return Loader.BitmapLoader.LoadTexture(filename, out width, out height);
		}

		public static float[][] LoadSprite(string filename, int width, int height)
		{
			if (!File.Exists(filename))
				throw new FileNotFoundException("The file `" + filename + "` has not been found.", filename);

			var result = new List<float[]>();

			using (var bmp = (Bitmap)System.Drawing.Image.FromFile(filename))
			{
				if (bmp.Width < width || bmp.Height < height)
					throw new Exception(string.Format("Given image bounds {0},{1} are bigger than the actual bounds {2},{3}.", width, height, bmp.Width, bmp.Height));

				var cWidth = (int)Math.Floor(bmp.Width / (float)width);
				var cHeight = (int)Math.Floor(bmp.Height / (float)height);

				var count = cWidth * cHeight;
				for (int c = 0; c < count; c++)
				{
					var ch = c / cWidth;
					var cw = c % cWidth;

					result.Add(Loader.BitmapLoader.LoadTexture(bmp, new Rectangle(cw * width, ch * height, width, height)));
				}
			}
			return result.ToArray();
		}
	}
}
