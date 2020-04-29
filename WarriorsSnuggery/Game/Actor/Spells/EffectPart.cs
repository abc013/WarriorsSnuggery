using WarriorsSnuggery.Spells;

namespace WarriorsSnuggery.Objects.Effects
{
	public class EffectPart : ITick
	{
		readonly Actor self;

		public readonly Spell Spell;

		int tick;
		int particleTick;
		public bool Active;

		public EffectPart(Actor self, Spell spell)
		{
			this.self = self;

			Spell = spell;
			Active = true;
			tick = spell.Duration;
			particleTick = spell.ParticleTick;

			if (Spell.Sound != null)
			{
				var sound = new Sound(Spell.Sound);
				sound.Play(self.Position, false);
			}
		}

		public void Tick()
		{
			if (Active && tick-- < 0)
				Active = false;

			if (Spell.Particles != null && particleTick-- < 0)
			{
				particleTick = Spell.ParticleTick;
				self.World.Add(Spell.Particles.Create(self.World, self.Position, self.Height));
			}
		}
	}
}
