﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace WarriorsSnuggery.Loader
{
	public static class BitmapLoader
	{
		public static List<float[]> LoadSprite(string filename, int width, int height)
		{
			if (!File.Exists(filename))
				throw new FileNotFoundException($"The file '{filename}' has not been found.");

			var result = new List<float[]>();

			using var bmp = (Bitmap)Image.FromFile(filename);

			if (bmp.Width < width || bmp.Height < height)
				throw new IndexOutOfRangeException($"Given image bounds ({width}, {height}) are bigger than the actual bounds ({bmp.Width}, {bmp.Height}).");

			var cWidth = (int)Math.Floor(bmp.Width / (float)width);
			var cHeight = (int)Math.Floor(bmp.Height / (float)height);

			var count = cWidth * cHeight;
			for (int c = 0; c < count; c++)
			{
				var ch = c / cWidth;
				var cw = c % cWidth;

				result.Add(LoadTexture(bmp, new Rectangle(cw * width, ch * height, width, height)));
			}

			return result;
		}

		public static float[] LoadTexture(string filename, out int width, out int height)
		{
			if (!File.Exists(filename))
				throw new FileNotFoundException($"The file '{filename}' has not been found.");

			using var bitmap = new Bitmap(filename);

			width = bitmap.Width;
			height = bitmap.Height;

			return LoadTexture(bitmap, new Rectangle(Point.Empty, bitmap.Size));
		}

		// https://stackoverflow.com/questions/4747428/getting-rgb-array-from-image-in-c-sharp original
		public static float[] LoadTexture(Bitmap bmp, Rectangle size)
		{
			const int pixelWidth = 4;

			var data = bmp.LockBits(size, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

			var scansize = data.Width * pixelWidth;
			var stride = data.Stride;

			var byteSize = data.Height * stride;
			GC.AddMemoryPressure(byteSize);

			var result = new float[byteSize];
			try
			{
				var scan = new byte[stride];
				for (int scanline = 0; scanline < data.Height; scanline++)
				{
					Marshal.Copy(IntPtr.Add(data.Scan0, scanline * stride), scan, 0, stride);

					for (int px = 0; px < data.Width; px++)
					{
						// little endian
						// B
						result[scanline * scansize + px * pixelWidth + 2] = scan[px * pixelWidth] / 255f;
						// G
						result[scanline * scansize + px * pixelWidth + 1] = scan[px * pixelWidth + 1] / 255f;
						// R
						result[scanline * scansize + px * pixelWidth] = scan[px * pixelWidth + 2] / 255f;
						// A
						result[scanline * scansize + px * pixelWidth + 3] = scan[px * pixelWidth + 3] / 255f;
					}
				}
			}
			finally
			{
				bmp.UnlockBits(data);
				GC.RemoveMemoryPressure(byteSize);
			}

			return result;
		}

		public static byte[] LoadBytes(string filename, out int width, out int height)
		{
			if (!File.Exists(filename))
				throw new FileNotFoundException($"The file '{filename}' has not been found.");

			using var bitmap = new Bitmap(filename);

			width = bitmap.Width;
			height = bitmap.Height;

			return LoadBytes(bitmap, new Rectangle(Point.Empty, bitmap.Size));
		}

		public static byte[] LoadBytes(Bitmap bmp, Rectangle size)
		{
			const int pixelWidth = 4;

			var data = bmp.LockBits(size, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
			var byteSize = data.Height * data.Stride;
			GC.AddMemoryPressure(byteSize);

			var result = new byte[data.Height * data.Width * pixelWidth];
			try
			{
				Marshal.Copy(data.Scan0, result, 0, data.Height * data.Width * pixelWidth);
			}
			finally
			{
				bmp.UnlockBits(data);
				GC.RemoveMemoryPressure(byteSize);
			}

			return result;
		}
	}
}
