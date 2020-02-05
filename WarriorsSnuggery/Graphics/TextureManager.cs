using OpenTK.Graphics.ES30;
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

		public static void DeleteTextures()
		{
			foreach (ITexture[] textureArray in textures.Values)
				foreach (ITexture texture in textureArray)
					texture.Dispose();

			textures.Clear();
		}

		static readonly Dictionary<string, ITexture[]> textures = new Dictionary<string, ITexture[]>();

		public static ITexture Texture(string filename, bool search = true)
		{
			if (search)
				filename = FileExplorer.FindIn(FileExplorer.Misc, filename);

			ITexture texture;
			if (textures.ContainsKey(filename))
			{
				textures.TryGetValue(filename, out ITexture[] textureArray);
				if (textureArray.Length == 1)
				{
					texture = textureArray[0];
					return texture;
				}
			}

			var data = loadTexture(filename, out int width, out int height);

			texture = createTexture(data, new TextureInfo(filename, TextureType.IMAGE, 10, width, height, false), filename);

			textures.Add(filename, new[] { texture });

			return texture;
		}

		public static ITexture NoiseTexture(MPos size, int depth = 8, float scale = 1f, int method = 0, bool colored = false, bool withAlpha = false, float intensity = 0, float contrast = 1)
		{
			float[] raw;
			switch (method)
			{
				default:
					raw = Noise.GenerateClouds(size, Program.SharedRandom, depth, scale);
					break;
				case 1:
					raw = Noise.GenerateNoise(size, Program.SharedRandom, 1f);
					break;
			}
			// Apply brightness and contrast
			for (int i = 0; i < raw.Length; i++)
			{
				raw[i] += intensity;
				raw[i] = (raw[i] - 0.5f) * contrast + 0.5f;
			}

			var data = new float[raw.Length * 4];

			for (int i = 0; i < raw.Length; i++)
			{
				if (colored)
				{
					data[(i * 4)] = raw[i];
					data[(i * 4) + 1] = raw[i + 1];
					data[(i * 4) + 2] = raw[i + 2];
					data[(i * 4) + 3] = withAlpha ? raw[i + 3] : 1.0f;
				}
				else
				{
					data[(i * 4)] = raw[i];
					data[(i * 4) + 1] = raw[i];
					data[(i * 4) + 2] = raw[i];
					data[(i * 4) + 3] = withAlpha ? raw[i] : 1.0f;
				}
			}
			return createTexture(data, new TextureInfo("", TextureType.IMAGE, 10, size.X, size.Y, false), "Random#" + Program.SharedRandom.GetHashCode());
		}

		public static ITexture[] Sprite(TextureInfo info)
		{
			var filename = info.File;

			if (string.IsNullOrEmpty(filename))
				return null;

			ITexture[] texture;

			if (textures.ContainsKey(filename))
			{
				textures.TryGetValue(filename, out texture);
				return texture;
			}

			var datas = loadSprite(filename, info.Width, info.Height);
			texture = new ITexture[datas.Length];
			for (int i = 0; i < datas.Length; i++)
			{
				var data = datas[i];
				texture[i] = createTexture(data, new TextureInfo(string.Empty, TextureType.ANIMATION, 10, info.Width, info.Height, false), filename);
			}
			textures.Add(filename, texture);

			return texture;
		}

		static void setTextureParams()
		{
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Nearest);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Nearest);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)All.Repeat);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)All.Repeat);
		}

		static ITexture createTexture(float[] data, TextureInfo info, string name)
		{
			lock (MasterRenderer.GLLock)
			{
				int id = GL.GenTexture();
				TextureCount++;

				Program.CheckGraphicsError("createTexture_1");

				GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
				GL.ActiveTexture(TextureUnit.Texture0);
				GL.BindTexture(TextureTarget.Texture2D, id);
				GL.TexImage2D(TextureTarget2d.Texture2D, 0, TextureComponentCount.Rgba32fExt, info.Width, info.Height, 0, PixelFormat.Rgba, PixelType.Float, data);

				setTextureParams();

				Program.CheckGraphicsError("createTexture_2");

				return new ITexture(name, info.Width, info.Height, id);
			}
		}

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
				GL.TexImage2D(TextureTarget2d.Texture2D, 0, TextureComponentCount.Rgba32fExt, size.X, size.Y, 0, PixelFormat.Rgba, PixelType.Float, data);

				setTextureParams();

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

		public static float[][] LoadCharacters(int fontSize, string fontName, out MPos maxSize, out MPos[] sizes)
		{
			var characters = new float[Font.Characters.Length][];
			sizes = new MPos[Font.Characters.Length];

			using (var font = new System.Drawing.Font(Font.Collection.Families.Where(a => a.Name == fontName).First(), fontSize))
			{
				var maxWidth = 0;
				var maxHeight = 0;
				for (int i = 0; i < Font.Characters.Length; i++)
				{
					var charBmp = generateFontChar(font, Font.Characters[i]);
					sizes[i] = new MPos(charBmp.Width, charBmp.Height);
					if (charBmp.Width > maxWidth)
						maxWidth = charBmp.Width;
					if (charBmp.Height > maxHeight)
						maxHeight = charBmp.Height;

					characters[i] = loadTexture(charBmp);
				}
				maxSize = new MPos(maxWidth, maxHeight);
			}

			return characters;
		}

		static Bitmap generateFontChar(System.Drawing.Font font, char c)
		{
			var size = getFontSize(font, c);
			var bmp = new Bitmap((int)Math.Ceiling(size.Width), (int)Math.Ceiling(size.Height));

			using (var gfx = System.Drawing.Graphics.FromImage(bmp))
			{
				gfx.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
				gfx.Clear(System.Drawing.Color.FromArgb(0));
				gfx.DrawString(c.ToString(), font, Brushes.White, 0, 0);
			}
			return bmp;
		}

		static SizeF getFontSize(System.Drawing.Font font, char c)
		{
			// If bugs start with font spacing, increase number of pixels here
			using (var bmp = new Bitmap(64, 64))
			{
				using (var gfx = System.Drawing.Graphics.FromImage(bmp))
				{
					return gfx.MeasureString(c.ToString(), font);
				}
			}
		}

		static float[] loadTexture(Bitmap bmp)
		{
			return Loader.BitmapLoader.LoadTexture(bmp, new Rectangle(Point.Empty, bmp.Size));
		}

		static float[] loadTexture(string filename, out int width, out int height)
		{
			if (!File.Exists(filename))
				throw new FileNotFoundException("The file `" + filename + "` has not been found.");

			return Loader.BitmapLoader.LoadTexture(filename, out width, out height);
		}

		public static float[][] loadSprite(string filename, int width, int height)
		{
			if (!File.Exists(filename))
				throw new FileNotFoundException("The file `" + filename + "` has not been found.", filename);

			var result = new List<float[]>();

			using (var bmp = (Bitmap)Image.FromFile(filename))
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
