namespace WarriorsSnuggery.Objects.Weapons
{
	public static class FalloffHelper
	{
		public static float GetMultiplier(float[] falloff, int[] steps, float dist)
		{
			var start = steps[0];

			for (int i = 1; i < steps.Length; i++)
			{
				var end = steps[i];

				if (end > dist)
					return (start - dist) / (end - start) * (falloff[i] - falloff[i - 1]) + falloff[i - 1];

				start = end;
			}

			return falloff[falloff.Length - 1];
		}

		public static float GetMax(float[] falloff, int[] steps, float maxValue)
		{
			if (falloff[falloff.Length - 1] != 0f)
				return float.PositiveInfinity;

			return steps[steps.Length - 1];
		}
	}
}
