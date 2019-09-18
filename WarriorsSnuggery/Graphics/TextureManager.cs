﻿/*
 * User: Andreas
 * Date: 11.08.2017
 * 
 */
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

		public static ITexture RandomTexture(TextureInfo info)
		{
			if (string.IsNullOrEmpty(info.File))
				return null;

			var textureArray = Sprite(info);
			ITexture texture = textureArray[Program.SharedRandom.Next(textureArray.Length)];

			return texture;
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
			texture = new ITexture[datas.Count];
			for (int i = 0; i < datas.Count; i++)
			{
				var data = datas[i];
				//for (int x = 0; x < data.Length; x++) data[x] = (float) Math.Sin(x * i); // HACK: just for fun
				//var noise = Noise.generateClouds(new MPos(data.Length / 4, 1), new Random());
				//for (int x = 0; x < data.Length; x++) { data[x++] = noise[x / 4]; data[x++] = noise[x  / 4]; data[x++] = noise[x / 4]; data[x] = 1f; } // HACK: just for fun
				texture[i] = createTexture(data, new TextureInfo(string.Empty, TextureType.ANIMATION, 10, info.Width, info.Height, false), filename);
			}
			textures.Add(filename, texture);

			return texture;
		}

		public static ITexture Font(string font, int size, out MPos maxSize, out int[] sizes)
		{
			maxSize = MPos.Zero;
			sizes = new int[Characters.Length];
			sizes.Initialize();

			if (string.IsNullOrEmpty(font))
				return null;

			ITexture texture;
			string name = font + size;

			// We don't need this as this method gets only called twice with different font.
			//if (textures.ContainsKey(name))
			//{
			//	textures.TryGetValue(name, out ITexture[] textureArray);
			//	texture = textureArray[0];
			//	maxSize = new MPos(texture.Width / Characters.Length, texture.Height);
			//	return texture;
			//}

			var bitmap = GenerateCharacters(size, font, out maxSize, out sizes);

			texture = createTexture(bitmap, name);

			// For dispose
			textures.Add(name, new[] { texture });

			return texture;
		}

		static ITexture createTexture(Bitmap bitmap, string name)
		{
			return createTexture(loadTexture(bitmap), new TextureInfo(name, TextureType.IMAGE, 0, bitmap.Width, bitmap.Height, false), name);
		}

		static ITexture createTexture(float[] data, TextureInfo info, string name)
		{
			lock (MasterRenderer.GLLock)
			{
				int id = GL.GenTexture();

				Program.CheckGraphicsError("createTexture_1");

				GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
				GL.ActiveTexture(TextureUnit.Texture0);
				GL.BindTexture(TextureTarget.Texture2D, id);
				GL.TexImage2D(TextureTarget2d.Texture2D, 0, TextureComponentCount.Rgba32fExt, info.Width, info.Height, 0, PixelFormat.Rgba, PixelType.Float, data);

				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Nearest);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Nearest);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)All.Repeat);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)All.Repeat);

				Program.CheckGraphicsError("createTexture_2");

				return new ITexture(name, info.Width, info.Height, id);
			}
		}

		public const string Characters = @" qwertyuiopasdfghjklzxcvbnmäöüQWERTYUIOPASDFGHJKLZXCVBNMÄÖÜ0123456789µ§!""#%&/()=?^*@${[]}\~¨'¯-_.:,;<>|°+↓↑←→";
		//public const string Characters = @" qwertyuiopasdfghjklzxcvbnmäöüQWERTYUIOPASDFGHJKLZXCVBNMÄÖÜ0123456789µ§½!""#¤%&/()=?^*@£¥€${[]}\~¨'¯-_.:,;<>|°©®±ツ+↓↑←→"; In order to save space and performance, we remove some chars we know we won't need.

		public static Bitmap GenerateCharacters(int fontSize, string fontName, out MPos maxSize, out int[] sizes)
		{
			var characters = new List<Bitmap>();
			sizes = new int[Characters.Length];

			Font font = null;
			var @private = IFont.Collection.Families.Where(a => a.Name == fontName);
			font = @private.Any() ? new Font(@private.First(), fontSize) : new Font(fontName, fontSize);

			for (int i = 0; i < Characters.Length; i++)
			{
				var charBmp = generateFontChar(font, Characters[i]);
				sizes[i] = charBmp.Width;
				characters.Add(charBmp);
			}
			maxSize = new MPos(characters.Max(x => x.Width), characters.Max(x => x.Height));
			var charMap = new Bitmap(maxSize.X * characters.Count, maxSize.Y);
			using (var gfx = System.Drawing.Graphics.FromImage(charMap))
			{
				gfx.FillRectangle(Brushes.Black, 0, 0, charMap.Width, charMap.Height);
				for (int i = 0; i < characters.Count; i++)
				{
					var c = characters[i];
					gfx.DrawImageUnscaled(c, i * maxSize.X, 0);

					c.Dispose();
				}
			}
			font.Dispose();
			return charMap;
		}

		static Bitmap generateFontChar(Font font, char c)
		{
			var size = getFontSize(font, c);
			var bmp = new Bitmap((int)Math.Ceiling(size.Width), (int)Math.Ceiling(size.Height));

			using (var gfx = System.Drawing.Graphics.FromImage(bmp))
			{
				gfx.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
				gfx.FillRectangle(Brushes.Black, 0, 0, bmp.Width, bmp.Height);
				gfx.DrawString(c.ToString(), font, Brushes.White, 0, 0);
			}
			return bmp;
		}

		static SizeF getFontSize(Font font, char c)
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

		static List<float[]> loadSprite(string filename, int width, int height)
		{
			if (!File.Exists(filename))
				throw new FileNotFoundException("The file `" + filename + "` has not been found.", filename);

			var result = new List<float[]>();

			using (var bmp = (Bitmap)Image.FromFile(filename))
			{
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
			return result;
		}
	}
}
