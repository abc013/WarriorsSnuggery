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
		SHIELD
	}

	public class Effect
	{
		public readonly EffectType Type;
		public readonly float Value;

		public readonly int ManaCost;

		public readonly int Duration;
		public readonly int RechargeDuration;

		public Effect(EffectType type, float value, int manaCost, int duration, int rechargeDuration)
		{
			Type = type;
			Value = value;

			ManaCost = manaCost;

			Duration = duration;
			RechargeDuration = rechargeDuration;
		}
	}
}
