using System.Collections.Generic;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Objects.Weather
{
	public enum WeatherEffectType
	{
		SNOW,
		RAIN
	}

	public class WeatherEffect
	{
		[Desc("Render type to use: SNOW renders squares, RAIN renders a line.")]
		public readonly WeatherEffectType Type;

		[Desc("Total particle count.")]
		public readonly int Particles = 800;

		[Desc("Color of the particles.")]
		public readonly Color Color = Color.White;

		[Desc("Color variation of the particles.")]
		public readonly Color ColorVariety;

		[Desc("Size of the particles.")]
		public readonly int Size = 64;

		[Desc("Size variation of the particles.")]
		public readonly int RandomSize;

		[Desc("Length of the particles.", "Only applied when using RAIN.")]
		public readonly int Length = 196;

		[Desc("Length variation of the particles.", "Only applied when using RAIN.")]
		public readonly int RandomLength;

		[Desc("Velocity of the particles.")]
		public readonly CPos Velocity;

		[Desc("Random velocity on top of the velocity of the particles.")]
		public readonly CPos RandomVelocity;

		[Desc("Random wind speed. This affects all particles at once.")]
		public readonly CPos WindVariation;

		[Desc("Determines how fast the wind direction will be changing.")]
		public readonly float WindChangeSpeed = 0.001f;

		public WeatherEffect(List<TextNode> nodes)
		{
			TypeLoader.SetValues(this, nodes);
		}
	}
}
