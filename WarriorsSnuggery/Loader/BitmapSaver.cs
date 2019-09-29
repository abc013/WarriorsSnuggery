using System.Drawing;
using System.Runtime.InteropServices;

namespace WarriorsSnuggery.Loader
{
	public class BitmapSaver
	{
		public static void Save(string filename, float[] data, MPos size)
		{
			byte[] data2 = new byte[data.Length];
			for (int i = 0; i < data.Length / 4; i++)
			{
				data2[i * 4 + 2] = (byte)(data[i * 4] * 255);
				data2[i * 4 + 1] = (byte)(data[i * 4 + 1] * 255);
				data2[i * 4] = (byte)(data[i * 4 + 2] * 255);
				data2[i * 4 + 3] = (byte)(data[i * 4 + 3] * 255);
			}

			using (var img = new Bitmap(size.X, size.Y, size.X * 4, System.Drawing.Imaging.PixelFormat.Format32bppArgb, Marshal.UnsafeAddrOfPinnedArrayElement(data2, 0)))
			{
				img.Save(filename);
			}
		}
	}
}
