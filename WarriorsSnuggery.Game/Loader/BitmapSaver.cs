using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;

namespace WarriorsSnuggery.Loader
{
	public class BitmapSaver
	{
		public static void Save(string filename, byte[] data, MPos size, bool invertY = false)
		{
			using var img = Image.LoadPixelData<Bgra32>(data, size.X, size.Y);

			if (invertY)
				img.Mutate(a => a.Flip(FlipMode.Horizontal));

			img.Save(filename);
		}
	}
}
