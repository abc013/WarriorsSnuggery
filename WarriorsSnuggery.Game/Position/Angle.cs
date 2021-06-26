using System;

namespace WarriorsSnuggery
{
	public static class Angle
	{
		public const float MaxRange = 2 * MathF.PI;

		public static float RandomAngle(Random random)
		{
			return (float)random.NextDouble() * MaxRange;
		}

		public static int ToDegree(float arc)
		{
			return Cast((int)(arc / MathF.PI * 180));
		}

		public static float ToArc(int degree)
		{
			return Cast(degree / 180f * MathF.PI);
		}

		public static float Cast(float arc)
		{
			if (arc < 0 || arc > MaxRange)
			{
				arc %= MaxRange;

				if (arc < 0)
					arc += MaxRange;
			}

			return arc;
		}

		public static int Cast(int degree)
		{
			degree %= 360;

			if (degree < 0)
				degree += 360;

			return degree;
		}

		public static float Diff(float angle1, float angle2)
		{
			var angle = angle1 - angle2;

			if (angle < -MathF.PI)
				angle += MaxRange;

			if (angle > MathF.PI)
				angle -= MaxRange;

			return angle;
		}

		public static float FromVector(int x, int y)
		{
			var angle = -MathF.Atan2(y, -x);

			if (angle < 0f)
				angle += MaxRange;

			return angle;
		}
	}
}
