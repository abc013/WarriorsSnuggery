using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Objects.Actors;

namespace WarriorsSnuggery.Spells
{
	public class SpellCaster : ITick
	{
		readonly Game game;
		readonly SpellCasterType node;

		int duration;
		int recharge;

		public SpellCasterState State { get; private set; }

		public float RemainingDuration => 1 - duration / (float)node.Duration;
		public float RechargeProgress => 1 - recharge / (float)node.Cooldown;

		readonly List<ActorEffect> currentEffects = new List<ActorEffect>();

		public SpellCaster(Game game, SpellCasterType node, (float, float) values)
		{
			this.game = game;
			this.node = node;

			if (values.Item1 != 0 || values.Item2 != 0)
			{
				duration = (int)((1 - values.Item1) * node.Duration);
				if (duration > 0)
					State = SpellCasterState.ACTIVE;

				recharge = (int)((1 - values.Item2) * node.Cooldown);
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

			if (game.Stats.Mana < node.ManaCost)
				return false;

			game.Stats.Mana -= node.ManaCost;

			State = SpellCasterState.SLEEPING;
			recharge = node.Cooldown;
			duration = node.Duration;

			currentEffects.Clear();
			currentEffects.AddRange(actor.CastSpell(node.Spell));

			return true;
		}

		public bool Unlocked()
		{
			if (node.Unlocked || Program.IgnoreTech)
				return true;

			return game.Stats.SpellUnlocked(node);
		}
	}
}
