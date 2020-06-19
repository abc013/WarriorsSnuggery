using WarriorsSnuggery.Spells;

namespace WarriorsSnuggery.Objects.Parts
{
	public class EffectPart : ITick
	{
		public readonly Spell Spell;
		readonly Actor self;

		int tick;
		int particleTick;
		public bool Active;

		public EffectPart(Actor self, Spell spell)
		{
			Spell = spell;
			this.self = self;
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
