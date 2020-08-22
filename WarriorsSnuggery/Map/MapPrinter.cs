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
	}
}
