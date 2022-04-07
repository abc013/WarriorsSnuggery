using System.Collections.Generic;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Spells;

namespace WarriorsSnuggery.Objects.Actors
{
	public class ActorEffect : ITick, INoticeMove
	{
		public readonly Effect Effect;
		readonly Actor self;

		readonly Sound sound;

		// HACK For saving
		readonly Spell spell;
		readonly int spellIndex;

		public bool Sleeping;
		public bool Active => !Sleeping && tick > 0;

		int sleepTick;
		int tick;

		public ActorEffect(Actor self, Spell spell, int spellIndex)
		{
			this.self = self;

			this.spell = spell;
			this.spellIndex = spellIndex;

			Effect = spell.Effects[spellIndex];

			tick = Effect.Duration;
			sleepTick = Effect.MaxSleepDuration;

			if (Effect.Sound != null)
				sound = new Sound(Effect.Sound);

			if (Effect.StartSound != null)
			{
				var sound = new Sound(Effect.StartSound);
				sound.Play(self.Position, false);
			}

			Sleeping = Effect.Activation != EffectActivationType.INSTANT;
		}

		public ActorEffect(Actor self, List<TextNode> nodes)
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
					case "Sleeping":
						Sleeping = child.Convert<bool>();

						break;
					case "SleepTick":
						sleepTick = child.Convert<int>();

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
				nameof(ActorEffect) + "=",
				"\tSpell=" + SpellCache.Types[spell],
				"\tSpellIndex=" + spellIndex,
				"\tTick=" + tick,
				"\tSleeping=" + Sleeping,
				"\tSleepTick=" + sleepTick
			};
		}

		public void Tick()
		{
			if (Sleeping)
			{
				if (Effect.MaxSleepDuration != 0 && sleepTick-- <= 0)
				{
					// Effect now is completely inactive
					Sleeping = false;
					tick = 0;
				}

				return;
			}

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
