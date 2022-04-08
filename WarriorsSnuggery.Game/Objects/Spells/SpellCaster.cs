using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Objects.Actors;

namespace WarriorsSnuggery.Spells
{
	public class SpellCaster : ITick
	{
		public readonly SpellCasterType Type;
		readonly Game game;

		public SpellCasterState State { get; private set; } = SpellCasterState.READY;

		public float RemainingDuration => 1 - duration / (float)Type.Duration;
		public float RechargeProgress => 1 - recharge / (float)Type.Cooldown;

		readonly List<ActorEffect> currentEffects = new List<ActorEffect>();

		int duration;
		int recharge;

		public SpellCaster(Game game, SpellCasterType type)
		{
			this.game = game;
			Type = type;

			(duration, recharge) = game.Save.GetSpellCasterValues(type.InnerName);

			if (duration > 0)
				State = SpellCasterState.ACTIVE;
			else if (recharge > 0)
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
			foreach (var effect in Type.Effects)
				currentEffects.Add(actor.CastEffect(effect));

			return true;
		}

		public bool Unlocked()
		{
			return game.Stats.SpellUnlocked(Type);
		}

		public List<string> Save()
		{
			var list = new List<string>();
			if (State == SpellCasterState.READY)
				return list;

			list.Add($"{Type.InnerName}=");
			list.Add($"\tRecharge={recharge}");
			list.Add($"\tDuration={duration}");

			return list;
		}
	}
}
