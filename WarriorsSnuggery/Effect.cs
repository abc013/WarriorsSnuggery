namespace WarriorsSnuggery
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
		ENEMY_COSTUM_EXPLOSION_DAMAGE,
		SHIELD
	}

	public enum EffectOccurTypes
	{
		ALWAYS,
		RANDOM,
		FIRST,
		ON_DAMAGE,
		ON_MOVE,
		ON_ATTACK,
		ON_KILLED
	}

	public class Effect
	{
		public readonly EffectType Type;
		public readonly float Value;
		public readonly int Duration;

		public readonly EffectOccurTypes Occurence;
		public readonly int Cooldown;

		public Effect(EffectType type, float value, int duration, EffectOccurTypes occurence, int reloadTick)
		{
			Type = type;
			Value = value;
			Duration = duration;
			Occurence = occurence;
			Cooldown = reloadTick;
		}
	}
}
