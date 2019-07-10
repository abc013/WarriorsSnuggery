/*
 * User: Andreas
 * Date: 14.07.2018
 * Time: 19:23
 */
using System;
using System.Collections.Generic;

namespace WarriorsSnuggery
{
	public static class Noise
	{
		public static float[] GenerateNoise(MPos size, Random random, float scale = 1f)
		{
			return GenerateNoise(size.X, size.Y, random, scale);
		}

		public static float[] GenerateNoise(int sizeX, int sizeY, Random random, float scale = 1f)
		{
			var raw = new float[sizeX * sizeY];

			for (int y = 0; y < sizeY; y++)
			{
				for (int x = 0; x < sizeX; x++)
				{
					raw[y * sizeX + x] = (float)random.NextDouble();
				}
			}

			var noise = new float[sizeX * sizeY];
			for (int y = 0; y < sizeY; y++)
			{
				for (int x = 0; x < sizeX; x++)
				{
					float superX = x / scale;
					float superY = y / scale;

					//get fractional part of x and y
					float fractX = superX - (int)superX;
					float fractY = superY - (int)superY;

					//wrap around
					int x1 = ((int)superX + sizeX) % sizeX;
					int y1 = ((int)superY + sizeY) % sizeY;

					//neighbor values
					int x2 = (x1 + sizeX - 1) % sizeX;
					int y2 = (y1 + sizeY - 1) % sizeY;

					//smooth the noise with bilinear interpolation
					float value = 0.0f;
					value += fractX * fractY * raw[y1 * sizeX + x1];
					value += (1 - fractX) * fractY * raw[y1 * sizeX + x2];
					value += fractX * (1 - fractY) * raw[y2 * sizeX + x1];
					value += (1 - fractX) * (1 - fractY) * raw[y2 * sizeX + x2];
					noise[y * sizeX + x] = value;
				}
			}

			return noise;
		}

		public static float[] GenerateClouds(MPos size, Random random, int depth = 2, float scale = 1f)
		{
			var noises = new List<float[]>();

			for (float i = depth; i >= 1; i++)
			{
				noises.Add(GenerateNoise(size.X, size.Y, random, i + (scale - 1)));
				i /= 2f;
				i--;
			}

			var noise = new float[size.X * size.Y];

			for (int i = 0; i < noise.Length; i++)
			{
				foreach (var n in noises)
					noise[i] += n[i];

				noise[i] /= noises.Count;
			}
			//for (int i = 0; i < noise.Length; i++)
			//	raw[i] = raw[i] < 0.5f ? 0 : 1; //HACK edit things here, so contrast a bit and then make it unsharp
			//	noise[i] = noise[i] * noise[i] * 2.5f;

			return noise;
		}
	}
}
