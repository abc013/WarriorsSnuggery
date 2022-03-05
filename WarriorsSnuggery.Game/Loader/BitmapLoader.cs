using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace WarriorsSnuggery.Loader
{
	public static class BitmapLoader
	{
		public static List<float[]> LoadSplit(string file, int width, int height)
		{
			if (!File.Exists(file))
				throw new FileNotFoundException($"The file '{file}' has not been found.");

			var result = new List<float[]>();

			using var img = Image.Load(file).CloneAs<RgbaVector>();
			var span = new Span<RgbaVector>(new RgbaVector[img.Width * img.Height]);
			img.CopyPixelDataTo(span);

			if (img.Width < width || img.Height < height)
				throw new IndexOutOfRangeException($"Given image bounds ({width}, {height}) are bigger than the actual bounds ({img.Width}, {img.Height}).");

			var cWidth = (int)Math.Floor(img.Width / (float)width);
			var cHeight = (int)Math.Floor(img.Height / (float)height);

			var count = cWidth * cHeight;
			for (int c = 0; c < count; c++)
			{
				var ch = c / cWidth;
				var cw = c % cWidth;

				result.Add(LoadSelection(span, (img.Width, img.Height), (cw * width, ch * height, width, height)));
			}

			return result;
		}

		public static float[] LoadWhole(string file, out int width, out int height)
		{
			if (!File.Exists(file))
				throw new FileNotFoundException($"The file '{file}' has not been found.");

			using var img = Image.Load(file).CloneAs<RgbaVector>();
			var span = new Span<RgbaVector>(new RgbaVector[img.Width * img.Height]);
			img.CopyPixelDataTo(span);

			width = img.Width;
			height = img.Height;

            return LoadSelection(span, (width, height), (0, 0, width, height));
		}

		internal static float[] LoadSelection(Span<RgbaVector> span, (int width, int height) bounds, (int x, int y, int w, int h) selection)
		{
			var result = new float[selection.w * selection.h * 4];

			for (int scanline = 0; scanline < selection.h; scanline++)
			{
				for (int pixel = 0; pixel < selection.w; pixel++)
				{
					var offset = (scanline * selection.w + pixel) * 4;
					var color = span[(selection.y + scanline) * bounds.width + selection.x + pixel];

					result[offset++] = color.B;
					result[offset++] = color.G;
					result[offset++] = color.R;
					result[offset++] = color.A;
				}
			}

			return result;
		}

		public static byte[] LoadBytes(string file, out int width, out int height)
		{
			if (!File.Exists(file))
				throw new FileNotFoundException($"The file '{file}' has not been found.");

			using var img = Image.Load(file).CloneAs<Rgba32>();

			width = img.Width;
			height = img.Height;

			var span = new Span<byte>(new byte[width * height * 4]);
			img.CopyPixelDataTo(span);
			return MemoryMarshal.AsBytes(span).ToArray();
		}
	}
}
