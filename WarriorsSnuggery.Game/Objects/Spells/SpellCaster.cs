using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Objects.Actors;

namespace WarriorsSnuggery.Spells
{
	public class SpellCaster : ITick
	{
		readonly Game game;
		readonly SpellCasterType type;

		int duration;
		int recharge;

		public SpellCasterState State { get; private set; }

		public float RemainingDuration => 1 - duration / (float)type.Duration;
		public float RechargeProgress => 1 - recharge / (float)type.Cooldown;

		readonly List<ActorEffect> currentEffects = new List<ActorEffect>();

		public SpellCaster(Game game, SpellCasterType type, (float, float) values)
		{
			this.game = game;
			this.type = type;

			if (values.Item1 != 0 || values.Item2 != 0)
			{
				duration = (int)((1 - values.Item1) * type.Duration);
				if (duration > 0)
					State = SpellCasterState.ACTIVE;

				recharge = (int)((1 - values.Item2) * type.Cooldown);
				if (recharge > 0)
					State = SpellCasterState.RECHARGING;
			}
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

			if (game.Stats.Mana < type.ManaCost)
				return false;

			game.Stats.Mana -= type.ManaCost;

			State = SpellCasterState.SLEEPING;
			recharge = type.Cooldown;
			duration = type.Duration;

			currentEffects.Clear();
			foreach (var spell in type.Effects)
				currentEffects.Add(actor.CastEffect(spell));

			return true;
		}

		public bool Unlocked()
		{
			return type.Unlocked || game.Stats.SpellUnlocked(type);
		}
	}
}
