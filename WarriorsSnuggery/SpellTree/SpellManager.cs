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
				spellCasters[i] = new SpellCaster(game, SpellTreeLoader.SpellTree[i]);
		}

		public void Tick()
		{
			foreach (var caster in spellCasters)
				caster.Tick();
		}

		public bool Activate(int caster)
		{
			return spellCasters[caster].Activate(game.World.LocalPlayer, MouseInput.GamePosition);
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

		int recharge;
		int duration;

		public bool Activated;
		public bool Recharging;
		public float RemainingDuration
		{
			get { return 1 - duration / (float)node.Spell.Duration; }
			set { }
		}
		public float RechargeProgress
		{
			get { return 1 - recharge / (float)node.Spell.Cooldown; }
			set { }
		}
		public bool Ready
		{
			get { return !(Activated || Recharging); }
			set { }
		}

		public SpellCaster(Game game, SpellTreeNode node)
		{
			this.game = game;
			this.node = node;
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

		public bool Activate(Actor actor, CPos position)
		{
			if (!Ready || !Unlocked())
				return false;

			if (game.Statistics.Mana < node.Spell.ManaCost)
				return false;

			game.Statistics.Mana -= node.Spell.ManaCost;

			Activated = true;
			recharge = node.Spell.Cooldown;
			duration = node.Spell.Duration;

			actor.CastSpell(node.Spell);

			return true;
		}

		public bool Unlocked()
		{
			if (node.Unlocked)
				return true;

			return game.Statistics.UnlockedSpells.ContainsKey(node.InnerName) && game.Statistics.UnlockedSpells[node.InnerName];
		}
	}
}
