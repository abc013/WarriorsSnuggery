using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.Objects
{
	public enum FalloffType
	{
		QUADRATIC,
		CUBIC,
		EXPONENTIAL,
		LINEAR,
		ROOT
	}

	public enum WeaponFireType
	{
		BULLET,
		ROCKET,
		BEAM,
	}

	public class WeaponType
	{
		public readonly TextureInfo Textures;
		public readonly TextureInfo Smudge;

		public readonly float Scale;

		public readonly int Damage;

		public readonly int Speed;
		public readonly int Acceleration;

		public readonly int Reload;

		public readonly ParticleType ParticlesWhenExplode;
		public readonly int ParticleCountWhenExplode;

		public readonly int Inaccuracy;

		public readonly int MaxRange;
		public readonly int MinRange;

		public readonly WeaponFireType WeaponFireType;
		public readonly FalloffType DamageFalloff;

		public readonly bool OrientateToTarget;

		public readonly Shape PhysicalShape;
		public readonly int PhysicalSize;

		public WeaponType(TextureInfo textures, TextureInfo smudge, float scale, int damage, int speed, int acceleration, int reload, ParticleType particlesWhenExplode, int particleCountWhenExplode, int inaccuracy, int maxRange, int minRange, FalloffType damageFalloff, WeaponFireType weaponFireType, bool orientateToTarget, Shape physicalShape, int physicalSize)
		{
			Textures = textures;
			Smudge = smudge;
			Damage = damage;
			Scale = scale;
			Speed = speed;
			Acceleration = acceleration;
			Reload = reload;
			ParticlesWhenExplode = particlesWhenExplode;
			ParticleCountWhenExplode = particleCountWhenExplode;
			Inaccuracy = inaccuracy;
			MaxRange = maxRange;
			MinRange = minRange;
			WeaponFireType = weaponFireType;
			DamageFalloff = damageFalloff;
			OrientateToTarget = orientateToTarget;
			PhysicalShape = physicalShape;
			PhysicalSize = physicalSize;
		}
	}
}
