using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Objects.Actors;

namespace WarriorsSnuggery.Spells
{
	public class SpellCaster : ITick, ILoadable, ISaveable
	{
		public readonly SpellCasterType Type;
		readonly Player player;

		public SpellCasterState State { get; private set; } = SpellCasterState.READY;

		public float RemainingDuration => 1 - duration / (float)Type.Duration;
		public float RechargeProgress => 1 - recharge / (float)Type.Cooldown;

		readonly List<ActorEffect> currentEffects = new List<ActorEffect>();

		[Save("Duration"), DefaultValue(0)]
		int duration;
		[Save("Recharge"), DefaultValue(0)]
		int recharge;

		public SpellCaster(Player player, SpellCasterType type)
		{
			this.player = player;
			Type = type;
		}

		public void Load(TextNodeInitializer initializer)
		{
			initializer.SetSaveFields(this);
			
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

			if (actor.IsPlayerSwitch)
				return false;

			if (player.Mana < Type.ManaCost)
				return false;

			player.Mana -= Type.ManaCost;

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
			return player.HasSpellUnlocked(Type);
		}

		public TextNodeSaver Save()
		{
			var saver = new TextNodeSaver();
			saver.AddSaveFields(this);

			return saver;
		}
	}
}
