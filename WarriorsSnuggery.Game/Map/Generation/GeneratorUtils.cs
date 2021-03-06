﻿namespace WarriorsSnuggery.Maps.Generators
{
	public static class GeneratorUtils
	{
		public static NoiseMap GetNoise(MapLoader loader, int id)
		{
			if (id < 0)
				return loader.EmptyNoiseMap;

			return loader.NoiseMaps[id];
		}

		public static float Multiplier(float[] probability, float[] steps, float value)
		{
			var start = steps[0];

			for (int i = 1; i < steps.Length; i++)
			{
				var end = steps[i];

				if (end > value)
					return (start - value) / (end - start) * (probability[i] - probability[i - 1]) + probability[i - 1];

				start = end;
			}

			return probability[^1];
		}
	}
}
