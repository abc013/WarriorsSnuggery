using System.Collections.Generic;
using WarriorsSnuggery.Spells;

namespace WarriorsSnuggery.Objects.Parts
{
	public class EffectPart : ITick
	{
		public readonly Effect Effect;
		readonly Actor self;

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
		}

		public EffectPart(Actor self, List<MiniTextNode> nodes)
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

			if (spellIndex >= spell.Effects.Length)
				tick = 0;
			else
				Effect = spell.Effects[spellIndex];
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
			if (self.World.Game.Editor)
				return;

			if (tick-- <= 0)
				return;

			if (Effect.Particles != null && tick % Effect.ParticleTick == 0)
				self.World.Add(Effect.Particles.Create(self.World, self.Position, self.Height));
		}
	}
}
