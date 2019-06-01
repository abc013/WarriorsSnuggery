using System;

namespace WarriorsSnuggery.Objects.Effects
{
	public class EffectPart : ITick
	{
		readonly Actor self;

		public readonly Effect Effect;

		int tick;
		public bool Active;

		public EffectPart(Actor self, Effect effect)
		{
			this.self = self;

			Effect = effect;
			Active = true;
			tick = effect.Duration;
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
