using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarriorsSnuggery.Objects.Weapons
{
	public static class FalloffHelper
	{
		public static float GetMultiplier(FalloffType type, float dist)
		{
			switch (type)
			{
				case FalloffType.LINEAR:
					return 1 / dist;
				case FalloffType.QUADRATIC:
					return 1 / (dist * dist);
				case FalloffType.CUBIC:
					return 1 / (dist * dist * dist);
				case FalloffType.EXPONENTIAL:
					return 0.1f / (float)Math.Pow(2, dist);
				case FalloffType.ROOT:
					return 1 / (float)Math.Sqrt(dist);
				default:
					return 1;
			}
		}
		public static float GetMax(FalloffType type, float maxValue)
		{
			switch (type)
			{
				case FalloffType.LINEAR:
					return maxValue;
				case FalloffType.QUADRATIC:
					return (float)Math.Sqrt(maxValue);
				case FalloffType.CUBIC:
					return (float)Math.Pow(maxValue, 1/3f);
				case FalloffType.EXPONENTIAL:
					return 10f / (float)Math.Log(maxValue, 2);
				case FalloffType.ROOT:
					return maxValue * maxValue;
				default:
					return 1;
			}
		}
	}
}
