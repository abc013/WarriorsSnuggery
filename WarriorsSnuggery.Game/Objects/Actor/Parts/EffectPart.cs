using System.Collections.Generic;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Spells;

namespace WarriorsSnuggery.Objects.Parts
{
	public class EffectPart : ITick, INoticeMove
	{
		public readonly Effect Effect;
		readonly Actor self;

		readonly Sound sound;

		// For saving
		readonly Spell spell;
		readonly int spellIndex;

		public bool Active => tick > 0;
		int tick;

		public EffectPart(Actor self, Effect effect, Spell spell, int spellIndex)
		{
			Effect = effect;
			this.self = self;

			this.spell = spell;
			this.spellIndex = spellIndex;

			tick = effect.Duration;

			if (effect.Sound != null)
				sound = new Sound(effect.Sound);
		}

		public EffectPart(Actor self, List<TextNode> nodes)
		{
			this.self = self;

			foreach(var child in nodes)
			{
				switch (child.Key)
				{
					case "Spell":
						spell = child.Convert<Spell>();

						break;
					case "SpellIndex":
						spellIndex = child.Convert<int>();

						break;
					case "Tick":
						tick = child.Convert<int>();

						break;
				}
			}

			Effect = spell.Effects[spellIndex];

			if (Effect.Sound != null)
				sound = new Sound(Effect.Sound);
		}

		public List<string> Save()
		{
			return new List<string>
			{
				nameof(EffectPart) + "=",
				"\tSpell=" + SpellCreator.GetName(spell),
				"\tSpellIndex=" + spellIndex,
				"\tTick=" + tick
			};
		}

		public void Tick()
		{
			if (tick-- <= 0)
				return;

			if (tick == Effect.Duration - 1)
				sound?.Play(self.Position, true);
			else if (tick == 0)
				sound?.Stop();

			if (Effect.Particles != null && tick % Effect.ParticleTick == 0)
				self.World.Add(Effect.Particles.Create(self.World, self.Position, self.Height));
		}

		public void OnMove(CPos old, CPos velocity)
		{
			sound?.SetPosition(self.Position);
		}
	}
}
