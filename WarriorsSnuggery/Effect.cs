using System;

namespace WarriorsSnuggery
{
	public enum EffectType
	{
		RANGE,
		INACCURACY,
		DAMAGE,
		COOLDOWN,
		HEALTH,
		MANA,
		SHIELD
	}

	public enum EffectOccurTypes
	{
		ALWAYS,
		RANDOM,
		ADDING,
		DIVIDING
	}

	public class Effect
	{
		public readonly EffectType Type;
		public readonly EffectOccurTypes Occurence;
		public readonly int Value1;
		public readonly int Value2;

		public Effect(EffectType type, EffectOccurTypes occurence, int value1, int value2)
		{
			Type = type;
			Occurence = occurence;
			Value1 = value1;
			Value2 = value2;
		}
	}
}
