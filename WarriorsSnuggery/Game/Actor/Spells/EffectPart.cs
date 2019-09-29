using WarriorsSnuggery.Spells;

namespace WarriorsSnuggery.Objects.Effects
{
	public class EffectPart : ITick
	{
		readonly Actor self;

		public readonly Spell Spell;

		int tick;
		public bool Active;

		public EffectPart(Actor self, Spell spell)
		{
			this.self = self;

			Spell = spell;
			Active = true;
			tick = spell.Duration;
		}

		public void Tick()
		{
			if (Active && tick-- < 0)
			{
				Active = false;
			}
		}
	}
}
