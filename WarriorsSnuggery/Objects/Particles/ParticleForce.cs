namespace WarriorsSnuggery.Objects.Particles
{
	public class ParticleForce
	{
		public readonly ParticleForceType Type;
		public readonly float Strength;
		public readonly bool UseHeight;

		public ParticleForce(ParticleForceType type, float strength, bool useHeight)
		{
			Type = type;
			Strength = strength;
			UseHeight = useHeight;
		}
	}
}
