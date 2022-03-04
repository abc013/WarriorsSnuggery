using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;
using WarriorsSnuggery.Maps.Noises;

namespace WarriorsSnuggery.Maps
{
	public static class MapPrinter
	{
		public static void PrintNoiseMap(MPos bounds, NoiseMap noise)
		{
			using var image = new Image<Rgba32>(bounds.X, bounds.Y);

			for (int x = 0; x < bounds.X; x++)
			{
				for (int y = 0; y < bounds.Y; y++)
				{
					var value = (int)(noise[x, y] * 255);
					image[x, y] = new Rgba32(value, value, value);
				}
			}
			var path = FileExplorer.Logs + "debugMaps/";
			checkDirectory(path);

			image.Save(path + $"noisemap{noise.ID}.png");
		}

		public static void PrintGeneratorMap(MPos bounds, NoiseMap noise, bool[,] dirty, int id)
		{
			using var image = new Image<Rgba32>(bounds.X, bounds.Y);

			for (int x = 0; x < bounds.X; x++)
			{
				for (int y = 0; y < bounds.Y; y++)
				{
					Rgba32 pixel;
					if (dirty[x, y])
						pixel = new Rgba32(255, 0, 0);
					else
					{
						var value = (int)(noise[x, y] * 255);
						pixel = new Rgba32(value, value, value);
					}

					image[x, y] = pixel;
				}
			}
			var path = FileExplorer.Logs + "debugMaps/";
			checkDirectory(path);

			image.Save(path + $"generator{id}.png");
		}

		static void checkDirectory(string path)
		{
			if (Directory.Exists(path))
				return;

			Directory.CreateDirectory(path);
		}
	}
}
