namespace WarriorsSnuggery.Objects.Weapons.Warheads
{
	public static class FalloffHelper
	{
		public static float GetMultiplier(float[] falloff, int[] steps, float dist, float modifier)
		{
			var start = (int)(steps[0] * modifier);

			for (int i = 1; i < steps.Length; i++)
			{
				var end = (int)(steps[i] * modifier);

				if (end > dist)
					return (start - dist) / (end - start) * (falloff[i] - falloff[i - 1]) + falloff[i - 1];

				start = end;
			}

			return falloff[^1];
		}

		public static int GetMax(float[] falloff, int[] steps)
		{
			if (falloff[^1] != 0f)
				return int.MaxValue;

			return steps[^1];
		}
	}
}
