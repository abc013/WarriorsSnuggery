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

			using var img = (Image<Rgba32>)Image.Load(file);

			if (img.Width < width || img.Height < height)
				throw new IndexOutOfRangeException($"Given image bounds ({width}, {height}) are bigger than the actual bounds ({img.Width}, {img.Height}).");

			var cWidth = (int)Math.Floor(img.Width / (float)width);
			var cHeight = (int)Math.Floor(img.Height / (float)height);

			var count = cWidth * cHeight;
			for (int c = 0; c < count; c++)
			{
				var ch = c / cWidth;
				var cw = c % cWidth;

				result.Add(LoadSelection(img, (cw * width, ch * height, width, height)));
			}

			return result;
		}

		public static float[] LoadWhole(string file, out int width, out int height)
		{
			if (!File.Exists(file))
				throw new FileNotFoundException($"The file '{file}' has not been found.");

			using var img = (Image<Rgba32>)Image.Load(file);

			width = img.Width;
			height = img.Height;

            return LoadSelection(img, (0, 0, width, height));
		}

		internal static float[] LoadSelection(Image<Rgba32> img, (int x, int y, int w, int h) selection)
		{
			var result = new float[selection.w * selection.h * 4];
			for (int scanline = 0; scanline < selection.h; scanline++)
			{
				var span = img.GetPixelRowSpan(selection.y + scanline);

				for (int pixel = 0; pixel < selection.w; pixel++)
				{
					var offset = (scanline * selection.w + pixel) * 4;
					var color = span[selection.x + pixel];

					// little endian
					result[offset + 2] = color.B / 255f; // B
					result[offset + 1] = color.G / 255f; // G
					result[offset]     = color.R / 255f; // R
					result[offset + 3] = color.A / 255f; // A
				}
			}

			return result;
		}

		public static byte[] LoadBytes(string file, out int width, out int height)
		{
			if (!File.Exists(file))
				throw new FileNotFoundException($"The file '{file}' has not been found.");

			using var img = (Image<Rgba32>)Image.Load(file);

			width = img.Width;
			height = img.Height;

			byte[] result;

			if (img.TryGetSinglePixelSpan(out var span))
				result = MemoryMarshal.AsBytes(span).ToArray();
			else
			{
				result = new byte[img.Width * img.Height * 4];
				Log.LoaderWarning("Image", $"Failed to load bytes of image (Source: '{file}'). Using black image instead.");
			}

			return result;
		}
	}
}
