namespace WarriorsSnuggery.Spells
{
	public enum EffectType
	{
		NONE,
		RANGE,
		INACCURACY,
		DAMAGE,
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
		[Desc("Type of the Spell.", "Available: NONE, RANGE, INACCURACY, DAMAGE, COOLDOWN, HEALTH, MANA, SPEED, SHIELD, STUN, INVISIBILITY")]
		public readonly EffectType Type;
		[Desc("Strength/Intensity of the spell.", "Value effect depends on the effect type.")]
		public readonly float Value;

		[Desc("Determines how much mana is used up by casting this spell.")]
		public readonly int ManaCost;

		[Desc("Duration of the spell.")]
		public readonly int Duration;
		[Desc("Cooldown of the spell.")]
		public readonly int Cooldown;

		public Spell(MiniTextNode[] nodes)
		{
			Loader.PartLoader.SetValues(this, nodes);
		}
	}
}
