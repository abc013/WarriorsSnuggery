using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.Spells
{
	public class SpellManager : ITick
	{
		readonly Game game;
		public readonly SpellCaster[] spellCasters;

		public SpellManager(Game game)
		{
			this.game = game;

			spellCasters = new SpellCaster[SpellTreeLoader.SpellTree.Count];
			for (int i = 0; i < spellCasters.Length; i++)
				spellCasters[i] = new SpellCaster(game, SpellTreeLoader.SpellTree[i], game.Stats.GetSpellCaster(i));
		}

		public void Tick()
		{
			foreach (var caster in spellCasters)
				caster.Tick();
		}

		public bool Activate(int caster)
		{
			return spellCasters[caster].Activate(game.World.LocalPlayer);
		}

		public bool Unlocked(int caster)
		{
			return spellCasters[caster].Unlocked();
		}
	}

	public class SpellCaster : ITick
	{
		readonly Game game;
		readonly SpellTreeNode node;

		int duration;
		int recharge;

		public bool Activated;
		public bool Recharging;

		public float RemainingDuration => 1 - duration / (float)node.Duration;
		public float RechargeProgress => 1 - recharge / (float)node.Cooldown;
		public bool Ready => !(Activated || Recharging);

		public SpellCaster(Game game, SpellTreeNode node, (float, float) values)
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
