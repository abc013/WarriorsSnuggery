using System;

namespace WarriorsSnuggery.Objects.Particles
{
	internal static class ParticleUtils
	{
		public static Random Random = Program.SharedRandom;

		public static Color Variety(Color value)
		{
			return new Color(Variety(value.R), Variety(value.G), Variety(value.B), Variety(value.A));
		}

		public static float Variety(float value)
		{
			return ((float)Random.NextDouble() - 0.5f) * value;
		}

		public static CPos Variety(CPos value)
		{
			return new CPos(radius(value.X), radius(value.Y), radius(value.Z));
		}

		public static VAngle AngleVariety(CPos value)
		{
			return new VAngle(radius(value.X), radius(value.Y), radius(value.Z));
		}

		static int radius(int radius)
		{
			return Random.Next(-radius, radius);
		}
	}
}
