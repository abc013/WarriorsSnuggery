using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace WarriorsSnuggery.Loader
{
	public static class BitmapLoader
	{
		public static float[] LoadTexture(string filename, out int width, out int height)
		{
			if (!File.Exists(filename))
				throw new FileNotFoundException("The file `" + filename + "` has not been found.");

			float[] r;

			using (var bitmap = new Bitmap(filename))
			{
				width = bitmap.Width;
				height = bitmap.Height;
				r = LoadTexture(bitmap, new Rectangle(Point.Empty, bitmap.Size));
			}

			return r;
		}

		public static float[] LoadTexture(string filename, int x, int y, int width, int height)
		{
			if (!File.Exists(filename))
				throw new FileNotFoundException("The file `" + filename + "` has not been found.");

			float[] r;

			using (var bitmap = new Bitmap(filename))
			{
				r = LoadTexture(bitmap, new Rectangle(x, y, width, height));
			}

			return r;
		}

		// https://stackoverflow.com/questions/4747428/getting-rgb-array-from-image-in-c-sharp original
		public static float[] LoadTexture(Bitmap bmp, Rectangle size)
		{
			const int pixelWidth = 4;

			var data = bmp.LockBits(size, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

			var scansize = data.Width * 4;
			var stride = data.Stride;
			var r = new float[data.Height * stride];
			try
			{
				var scan = new byte[stride];
				for (int scanline = 0; scanline < data.Height; scanline++)
				{
					Marshal.Copy(data.Scan0 + scanline * stride, scan, 0, stride);
					for (int px = 0; px < data.Width; px++)
					{
						// little endian
						// B
						r[scanline * scansize + px * pixelWidth + 2] = scan[px * pixelWidth] / 255f;
						// G
						r[scanline * scansize + px * pixelWidth + 1] = scan[px * pixelWidth + 1] / 255f;
						// R
						r[scanline * scansize + px * pixelWidth] = scan[px * pixelWidth + 2] / 255f;
						// A
						r[scanline * scansize + px * pixelWidth + 3] = scan[px * pixelWidth + 3] / 255f;
					}
				}
			}
			finally
			{
				bmp.UnlockBits(data);
			}

			return r;
		}
	}
}
