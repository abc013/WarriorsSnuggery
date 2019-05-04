using System;
using System.Diagnostics;
using System.Drawing;

namespace WarriorsSnuggery.Maps
{
	public static class MapPrinter
	{
		public static void PrintMapGeneration(string map, int evolution, int[,] generatedTerrain)
		{
			using(var image = new Bitmap(generatedTerrain.GetLength(0), generatedTerrain.GetLength(1)))
			{
				for (int x = 0; x < image.Width; x++)
				{
					for (int y = 0; y < image.Height; y++)
					{
						var value = (generatedTerrain[x, y] * 255) / 8;
						var color = System.Drawing.Color.FromArgb(value, value, value);
						image.SetPixel(x, y, color);
					}
				}
				image.Save(FileExplorer.Logs + "debugMaps/" + map + "_" + evolution + ".png");
				//Process.Start(FileExplorer.Logs + "debugMaps/" + map + "_" + evolution + ".png");
				image.Dispose();
			}
		}
		public static void PrintMapGeneration(string map, int evolution, int[,] generatedTerrain, bool[,] usedTerrain)
		{
			using (var image = new Bitmap(generatedTerrain.GetLength(0), generatedTerrain.GetLength(1)))
			{
				for (int x = 0; x < image.Width; x++)
				{
					for (int y = 0; y < image.Height; y++)
					{
						if (!usedTerrain[x, y])
						{
							var value = (generatedTerrain[x, y] * 255) / 8;
							var color = System.Drawing.Color.FromArgb(value, value, value);
							image.SetPixel(x, y, color);
						}
						else
						{
							image.SetPixel(x, y, System.Drawing.Color.Green);
						}
					}
				}
				image.Save(FileExplorer.Logs + "debugMaps/" + map + "_" + evolution + ".png");
				//Process.Start(FileExplorer.Logs + "debugMaps/" + map + "_" + evolution + ".png");
				image.Dispose();
			}
		}
	}
}
