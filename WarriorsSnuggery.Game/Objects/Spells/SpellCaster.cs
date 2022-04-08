using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Objects.Actors;

namespace WarriorsSnuggery.Spells
{
	public class SpellCaster : ITick
	{
		public readonly SpellCasterType Type;
		readonly Game game;

		public SpellCasterState State { get; private set; }

		public float RemainingDuration => 1 - duration / (float)Type.Duration;
		public float RechargeProgress => 1 - recharge / (float)Type.Cooldown;

		readonly List<ActorEffect> currentEffects = new List<ActorEffect>();

		int duration;
		int recharge;

		public SpellCaster(Game game, SpellCasterType type)
		{
			this.game = game;
			this.Type = type;

			var (currentDuration, currentRecharge) = game.Stats.GetSpellCasterValues(type.InnerName);

			duration = (int)(currentDuration * type.Duration);
			if (duration > 0)
				State = SpellCasterState.ACTIVE;

			recharge = (int)(currentRecharge * type.Cooldown);
			if (recharge > 0)
				State = SpellCasterState.RECHARGING;
		}

		public void Tick()
		{
			if (State == SpellCasterState.SLEEPING)
			{
				if (!currentEffects.Any(a => a.Sleeping || a.Active))
					State = SpellCasterState.RECHARGING;
				else if (!currentEffects.Any(a => a.Sleeping))
					State = SpellCasterState.ACTIVE;
			}

			if (State == SpellCasterState.ACTIVE && duration-- <= 0)
				State = SpellCasterState.RECHARGING;

			if (State == SpellCasterState.RECHARGING && recharge-- <= 0)
				State = SpellCasterState.READY;
		}

		public void CancelActive()
		{
			if (State == SpellCasterState.ACTIVE)
			{
				duration = 0;
				State = SpellCasterState.RECHARGING;
			}
		}

		public bool Activate(Actor actor)
		{
			if (State != SpellCasterState.READY || !Unlocked())
				return false;

			if (game.World.LocalPlayer.IsPlayerSwitch)
				return false;

			if (game.Stats.Mana < Type.ManaCost)
				return false;

			game.Stats.Mana -= Type.ManaCost;

			State = SpellCasterState.SLEEPING;
			recharge = Type.Cooldown;
			duration = Type.Duration;

			currentEffects.Clear();
			foreach (var spell in Type.Effects)
				currentEffects.Add(actor.CastEffect(spell));

			return true;
		}

		public bool Unlocked()
		{
			return game.Stats.SpellUnlocked(Type);
		}
	}
}
