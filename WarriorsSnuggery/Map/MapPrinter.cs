using System.Drawing;

namespace WarriorsSnuggery.Maps
{
	public static class MapPrinter
	{
		public static void PrintNoiseMap(NoiseMap map)
		{
			using var image = new Bitmap(map.Bounds.X, map.Bounds.Y);

			for (int x = 0; x < map.Bounds.X; x++)
			{
				for (int y = 0; y < map.Bounds.Y; y++)
				{
					var value = (int)(map.Values[x * map.Bounds.Y + y] * 255);
					var color = System.Drawing.Color.FromArgb(value, value, value);

					image.SetPixel(x, y, color);
				}
			}
			image.Save(FileExplorer.Logs + "debugMaps/noisemap" + map.ID + ".png");
		}

		public static void PrintGeneratorMap(MPos bounds, float[] noise, bool[,] dirty, int id)
		{
			using var image = new Bitmap(bounds.X, bounds.Y);

			for (int x = 0; x < bounds.X; x++)
			{
				for (int y = 0; y < bounds.Y; y++)
				{
					System.Drawing.Color color = Color.Red;
					if (dirty[x, y])
					{
						var value = (int)(noise[x * bounds.Y + y] * 255);
						color = System.Drawing.Color.FromArgb(value, value, value);
					}

					image.SetPixel(x, y, color);
				}
			}
			image.Save(FileExplorer.Logs + "debugMaps/generator" + id + ".png");
		}
	}
}
