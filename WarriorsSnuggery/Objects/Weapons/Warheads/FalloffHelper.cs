using OpenToolkit.Graphics.OpenGL;

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
