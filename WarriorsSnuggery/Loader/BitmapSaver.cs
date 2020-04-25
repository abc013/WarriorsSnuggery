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
			if (invertY)
			{
				for (int h = 0; h < size.Y; h++)
				{
					var offset1 = h * size.X * 4;
					var offset2 = length - offset1 - size.X * 4;
					for (int i = 0; i < size.X; i++)
					{
						data2[offset2 + i * 4 + 2] = (byte)(data[offset1 + i * 4] * 255);
						data2[offset2 + i * 4 + 1] = (byte)(data[offset1 + i * 4 + 1] * 255);
						data2[offset2 + i * 4] = (byte)(data[offset1 + i * 4 + 2] * 255);
						data2[offset2 + i * 4 + 3] = (byte)(data[offset1 + i * 4 + 3] * 255);
					}
				}
			}
			else
			{
				for (int i = 0; i < length / 4; i++)
				{
					data2[i * 4 + 2] = (byte)(data[i * 4] * 255);
					data2[i * 4 + 1] = (byte)(data[i * 4 + 1] * 255);
					data2[i * 4] = (byte)(data[i * 4 + 2] * 255);
					data2[i * 4 + 3] = (byte)(data[i * 4 + 3] * 255);
				}
			}

			using var img = new Bitmap(size.X, size.Y, size.X * 4, System.Drawing.Imaging.PixelFormat.Format32bppArgb, Marshal.UnsafeAddrOfPinnedArrayElement(data2, 0));

			img.Save(filename);
		}
	}
}
