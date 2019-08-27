using System;

namespace WarriorsSnuggery.Objects.Particles
{
	public class ParticleForce
	{
		public readonly ParticleForceType Type;
		public readonly float Strength;

		public ParticleForce(ParticleForceType type, float strength)
		{
			Type = type;
			Strength = strength;
		}
	}
}
