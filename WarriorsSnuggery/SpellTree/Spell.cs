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
		SHIELD
	}

	public class Spell
	{
		[Desc("Type of the Spell.", "Available: NONE, RANGE, INACCURACY, DAMAGE, COOLDOWN, HEALTH, MANA, SPEED, SHIELD")]
		public readonly EffectType Type;
		[Desc("Strength/Intensity of the spell.", "Value effect depends on the effect type.")]
		public readonly float Value;

		[Desc("Determines how much mana is used up by casting this spell.")]
		public readonly int ManaCost;

		[Desc("Duration of the spell.")]
		public readonly int Duration;
		[Desc("Cooldown of the spell.")]
		public readonly int Cooldown;
		[Desc("Key combination to press in order to cast this spell.")]
		public readonly string[] KeyCombo;

		public Spell(MiniTextNode[] nodes)
		{
			Loader.PartLoader.SetValues(this, nodes);
		}

		public string GetKeyComboString()
		{
			string str = "";
			if (KeyCombo != null)
			{
				for (int i = 0; i < KeyCombo.Length; i++)
				{
					str += Color.Green + KeyCombo[i];
					if (i < KeyCombo.Length - 1)
						str += Color.Grey + ",";
				}
			}

			return str;
		}
	}
}
