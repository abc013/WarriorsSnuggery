using WarriorsSnuggery.Objects.Particles;

namespace WarriorsSnuggery.Spells
{
	public enum EffectType
	{
		NONE,
		RANGE,
		INACCURACY,
		DAMAGE,
		DAMAGERANGE,
		COOLDOWN,
		HEALTH,
		MANA,
		SPEED,
		SHIELD,
		STUN,
		INVISIBILITY
	}

	public class Spell
	{
		[Desc("Type of the Spell.")]
		public readonly EffectType Type;
		[Desc("Strength/Intensity of the spell.", "Value effect depends on the effect type.")]
		public readonly float Value = 1f;

		[Desc("Determines how much mana is used up by casting this spell.")]
		public readonly int ManaCost;

		[Desc("Duration of the spell.")]
		public readonly int Duration;
		[Desc("Cooldown of the spell.")]
		public readonly int Cooldown;

		[Desc("Spawns particles while the spell is active.")]
		public readonly ParticleSpawner Particles;
		[Desc("Frequency of spawning Particles.")]
		public readonly int ParticleTick;
		[Desc("Play sound when triggered.")]
		public readonly SoundType Sound;

		public Spell(MiniTextNode[] nodes)
		{
			Loader.PartLoader.SetValues(this, nodes);
		}
	}
}
