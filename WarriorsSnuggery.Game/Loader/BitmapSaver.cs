using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;

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
			using var img = Image.LoadPixelData<Bgra32>(data, size.X, size.Y);

			if (invertY)
				img.Mutate(a => a.Flip(FlipMode.Horizontal));

			img.Save(filename);
		}
	}
}
