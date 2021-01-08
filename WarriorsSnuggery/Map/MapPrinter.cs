using System.Drawing;
using System.IO;

namespace WarriorsSnuggery.Maps
{
	public static class MapPrinter
	{
		public static void PrintNoiseMap(MPos bounds, NoiseMap map)
		{
			using var image = new Bitmap(bounds.X, bounds.Y);

			for (int x = 0; x < bounds.X; x++)
			{
				for (int y = 0; y < bounds.Y; y++)
				{
					var value = (int)(map[x, y] * 255);
					var color = System.Drawing.Color.FromArgb(value, value, value);

					image.SetPixel(x, y, color);
				}
			}
			var path = FileExplorer.Logs + "debugMaps/";
			checkDirectory(path);

			image.Save(path + $"noisemap{map.ID}.png");
		}

		public static void PrintGeneratorMap(MPos bounds, NoiseMap noise, bool[,] dirty, int id)
		{
			using var image = new Bitmap(bounds.X, bounds.Y);

			for (int x = 0; x < bounds.X; x++)
			{
				for (int y = 0; y < bounds.Y; y++)
				{
					System.Drawing.Color color = Color.Red;
					if (!dirty[x, y])
					{
						var value = (int)(noise[x, y] * 255);
						color = System.Drawing.Color.FromArgb(value, value, value);
					}

					image.SetPixel(x, y, color);
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
