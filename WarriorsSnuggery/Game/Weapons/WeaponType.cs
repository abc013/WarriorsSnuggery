using WarriorsSnuggery.Graphics;
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
		public readonly TextureInfo Texture;
		[Desc("Texture of the Weapon.", "Used in Beamweapons as startup.")]
		public readonly TextureInfo TextureStart;
		[Desc("Texture of the Weapon.", "Used in Beamweapons for the end.")]
		public readonly TextureInfo TextureEnd;
		[Desc("Texture of the Smudge that will be left behind from impact.")]
		public readonly TextureInfo Smudge;

		[Desc("Highest damage value possible.")]
		public readonly int Damage;
		[Desc("Highest damage value for walls.")]
		public readonly int WallDamage;

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
		public readonly int MaxRange = 8192;
		[Desc("Minimal Range the player can target.")]
		public readonly int MinRange;

		[Desc("Type of weapon.", "Possible: BULLET, ROCKET, BEAM, DIRECTEDBEAM")]
		public readonly WeaponFireType WeaponFireType = WeaponFireType.BULLET;
		[Desc("Falloff of the impact.", "Possible: QUADRATIC, CUBIC, EXPONENTIAL, LINEAR, ROOT")]
		public readonly FalloffType Falloff = FalloffType.QUADRATIC;

		[Desc("Weapon always points to the target.")]
		public readonly bool OrientateToTarget;

		[Desc("Collision shape of the weapon.", "Possible: CIRCLE, RECTANGLE, LINE_HORIZONTAL, LINE_VERTICAL, NONE")]
		public readonly Physics.Shape PhysicalShape = Physics.Shape.RECTANGLE;
		[Desc("Size of the collision boundary.")]
		public readonly int PhysicalSize = 64;

		[Desc("Gravity applied to the weapon.")]
		public readonly int Gravity = 9;

		[Desc("Interval in which the beam will detonate its warhead.", "Works only if the WeaponFireType is BEAM or DIRECTEDBEAM.")]
		public readonly int BeamImpactInterval;

		[Desc("Determines how long a beam will be fired.")]
		public readonly int BeamDuration;
		[Desc("Determines how the beam needs to be fired up.")]
		public readonly int BeamStartDuration;
		[Desc("Determines how long the beam stays after ending.")]
		public readonly int BeamEndDuration;

		public WeaponType(string name, MiniTextNode[] nodes)
		{
			Loader.PartLoader.SetValues(this, nodes);

			if (name != "DocWriterTest")
			{
				if (Texture == null)
					throw new YamlMissingNodeException(name, "Textures");

				SpriteManager.AddTexture(Texture);
				if (TextureEnd != null)
					SpriteManager.AddTexture(TextureEnd);
				if (TextureStart != null)
					SpriteManager.AddTexture(TextureStart);
				if (Smudge != null)
					SpriteManager.AddTexture(Smudge);
			}
		}
	}
}
