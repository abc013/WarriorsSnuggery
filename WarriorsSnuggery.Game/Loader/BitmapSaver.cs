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
				img.Mutate(a => a.Flip(FlipMode.Vertical).ProcessPixelRowsAsVector4(row =>
				{
					for (int x = 0; x < row.Length; x++)
						row[x].W = 1;
				}));

			img.SaveAsync(filename);
		}
	}
}
