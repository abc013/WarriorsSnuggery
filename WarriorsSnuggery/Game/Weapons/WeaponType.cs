﻿using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects.Particles;

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
		DIRECTEDBEAM
	}

	public class WeaponType
	{
		[Desc("Texture of the Weapon.")]
		public readonly TextureInfo Textures;
		[Desc("Texture of the Smudge that will be left behind from impact.")]
		public readonly TextureInfo Smudge;

		[Desc("Highest damage value possible.")]
		public readonly int Damage;

		[Desc("Speed of the warhead.")]
		public readonly int Speed;
		[Desc("Acceleration of the warhead.")]
		public readonly int Acceleration;

		[Desc("Time until the weapon has been reloaded.", "Will also be used for the duration of beam weapons.")]
		public readonly int Reload;

		[Desc("Particles that will be emitted when the weapon impacts.")]
		public readonly ParticleSpawner ParticlesOnImpact;

		[Desc("Inaccuracy of the weapon.")]
		public readonly int Inaccuracy;

		[Desc("Maximal Range the weapon can travel.")]
		public readonly int MaxRange;
		[Desc("Minimal Range the player can target.")]
		public readonly int MinRange;

		[Desc("Type of weapon.", "Possible: BULLET, ROCKET, BEAM, DIRECTEDBEAM")]
		public readonly WeaponFireType WeaponFireType;
		[Desc("Falloff of the impact.", "Possible: QUADRATIC, CUBIC, EXPONENTIAL, LINEAR, ROOT")]
		public readonly FalloffType DamageFalloff;

		[Desc("Weapon always points to the target.")]
		public readonly bool OrientateToTarget;

		[Desc("Collision shape of the weapon.", "Possible: CIRCLE, RECTANGLE, LINE_HORIZONTAL, LINE_VERTICAL, NONE")]
		public readonly Physics.Shape PhysicalShape;
		[Desc("Size of the collision boundary.")]
		public readonly int PhysicalSize;

		[Desc("Gravity applied to the weapon.")]
		public readonly int Gravity;

		[Desc("Interval in which the beam will detonate its warhead.", "Works only if the WeaponFireType is BEAM or DIRECTEDBEAM.")]
		public readonly int BeamImpactInterval;
		public WeaponType(TextureInfo textures, TextureInfo smudge, int damage, int speed, int acceleration, int reload, ParticleSpawner particlesOnImpact, int beamImpactInterval, int inaccuracy, int maxRange, int minRange, FalloffType damageFalloff, WeaponFireType weaponFireType, bool orientateToTarget, Physics.Shape physicalShape, int physicalSize, int gravity)
		{
			Textures = textures;
			Smudge = smudge;
			Damage = damage;
			Speed = speed;
			Acceleration = acceleration;
			Reload = reload;
			ParticlesOnImpact = particlesOnImpact;
			BeamImpactInterval = beamImpactInterval;
			Inaccuracy = inaccuracy;
			MaxRange = maxRange;
			MinRange = minRange;
			WeaponFireType = weaponFireType;
			DamageFalloff = damageFalloff;
			OrientateToTarget = orientateToTarget;
			PhysicalShape = physicalShape;
			PhysicalSize = physicalSize;
			Gravity = gravity;

			SpriteManager.AddTexture(Textures);
			if (Smudge != null)
				SpriteManager.AddTexture(Smudge);
		}
	}
}