using WarriorsSnuggery.Objects.Actors;

namespace WarriorsSnuggery.Spells
{
	public class SpellCasterManager : ITick
	{
		readonly Game game;
		public readonly SpellCaster[] Casters;

		public SpellCasterManager(Game game)
		{
			this.game = game;

			Casters = new SpellCaster[SpellCasterCache.Types.Count];
			for (int i = 0; i < Casters.Length; i++)
				Casters[i] = new SpellCaster(game, SpellCasterCache.Types[i], game.Stats.GetSpellCaster(i));
		}

		public void Tick()
		{
			foreach (var caster in Casters)
				caster.Tick();
		}

		public bool Activate(int caster)
		{
			return Casters[caster].Activate(game.World.LocalPlayer);
		}

		public bool Unlocked(int caster)
		{
			return Casters[caster].Unlocked();
		}
	}

	public class SpellCaster : ITick
	{
		readonly Game game;
		readonly SpellCasterType node;

		int duration;
		int recharge;

		public bool Activated;
		public bool Recharging;

		public float RemainingDuration => 1 - duration / (float)node.Duration;
		public float RechargeProgress => 1 - recharge / (float)node.Cooldown;
		public bool Ready => !(Activated || Recharging);

		public SpellCaster(Game game, SpellCasterType node, (float, float) values)
		{
			this.game = game;
			this.node = node;

			if (values.Item1 != 0 || values.Item2 != 0)
			{
				duration = (int)((1 - values.Item1) * node.Duration);
				recharge = (int)((1 - values.Item2) * node.Cooldown);
				if (duration > 0)
					Activated = true;
				else if (recharge > 0)
					Recharging = true;
			}
		}

		public void Tick()
		{
			if (Activated && duration-- <= 0)
			{
				Recharging = true;
				Activated = false;
			}
			if (Recharging && recharge-- <= 0)
			{
				Recharging = false;
			}
		}

		public bool Activate(Actor actor)
		{
			if (!Ready || !Unlocked())
				return false;

			if (game.Stats.Mana < node.ManaCost)
				return false;

			game.Stats.Mana -= node.ManaCost;

			Activated = true;
			recharge = node.Cooldown;
			duration = node.Duration;

			actor.CastSpell(node.Spell);

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
