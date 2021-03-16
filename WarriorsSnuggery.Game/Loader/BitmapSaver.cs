using System.Drawing;
using System.Runtime.InteropServices;

namespace WarriorsSnuggery.Loader
{
	public class BitmapSaver
	{
		public static void Save(string filename, float[] data, MPos size, bool invertY = false)
		{
			var length = data.Length;
			var data2 = new byte[length];
			for (int i = 0; i < length / 4; i++)
			{
				data2[i * 4 + 2] = (byte)(data[i * 4] * 255);
				data2[i * 4 + 1] = (byte)(data[i * 4 + 1] * 255);
				data2[i * 4] = (byte)(data[i * 4 + 2] * 255);
				data2[i * 4 + 3] = (byte)(data[i * 4 + 3] * 255);
			}

			Save(filename, data2, size, invertY);
		}

		public static void Save(string filename, byte[] data, MPos size, bool invertY = false)
		{
			using var img = new Bitmap(size.X, size.Y, size.X * 3, System.Drawing.Imaging.PixelFormat.Format24bppRgb, Marshal.UnsafeAddrOfPinnedArrayElement(data, 0));

			if (invertY)
				img.RotateFlip(RotateFlipType.RotateNoneFlipY);

			img.Save(filename);
		}
	}
}
