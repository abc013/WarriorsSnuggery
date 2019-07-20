using System.Drawing;

namespace WarriorsSnuggery.Maps
{
	public static class MapPrinter
	{
		public static void PrintMapGeneration(string map, int[,] generatedTerrain, int[,] generatorTerrain, int generatorIDs)
		{
			using (var image = new Bitmap(generatorTerrain.GetLength(0), generatorTerrain.GetLength(1)))
			{
				if (generatorIDs != 0)
				{
					for (int x = 0; x < image.Width; x++)
					{
						for (int y = 0; y < image.Height; y++)
						{
							var value = (generatorTerrain[x, y] * 255) / generatorIDs;
							var color = System.Drawing.Color.FromArgb(value, value, value);
							image.SetPixel(x, y, color);
						}
					}
				}
				image.Save(FileExplorer.Logs + "debugMaps/" + map + "_generator.png");
				image.Dispose();
			}
			using (var image = new Bitmap(generatedTerrain.GetLength(0), generatedTerrain.GetLength(1)))
			{
				for (int x = 0; x < image.Width; x++)
				{
					for (int y = 0; y < image.Height; y++)
					{
						var value = (generatedTerrain[x, y] * 255) / TerrainCreator.GetIDs().Length;
						var color = System.Drawing.Color.FromArgb(value, value, value);
						image.SetPixel(x, y, color);
					}
				}
				image.Save(FileExplorer.Logs + "debugMaps/" + map + "_terrain.png");
				image.Dispose();
			}
		}
	}
}
