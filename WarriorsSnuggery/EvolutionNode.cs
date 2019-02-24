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
		FIRST,
		RANDOM,
		ALWAYS,
		ALWAYS_ADDING,
		ALWAYS_MULTIPLYING,
		ALWAYS_DIVIDING
	}

	public class Effect
	{
		public readonly EffectType Type;
		public readonly EffectOccurTypes Occurence;
		public readonly int Value;

		public Effect(EffectType type, EffectOccurTypes occurence, int value)
		{
			Type = type;
			Occurence = occurence;
			Value = value;
		}
	}

	public class EvolutionNode
	{
		public readonly EvolutionNode[] After;
		public readonly int Cost;
		public bool Unlocked;

		public string Name;
		public string Effect;

		public EvolutionNode()
		{

		}
	}
}
