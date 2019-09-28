using System;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI
{
	public class SpellListItem : PanelItem
	{
		readonly SpellTreeNode node;
		readonly Game game;

		int recharge;
		int duration;
		bool activated;
		readonly bool exists;

		public SpellListItem(CPos pos, MPos size, SpellTreeNode node, Game game) : base(pos, new ImageRenderable(TextureManager.Texture(node.Icon)), size, node.Name, new[] { Color.Grey + "Mana use: " + new Color(0.5f, 0.5f, 1f) + node.Spell.ManaCost, Color.Grey + "Reload: " + Color.Green + Math.Round(node.Spell.RechargeDuration / (float)Settings.UpdatesPerSecond, 2) + Color.Grey + " Seconds" }, null)
		{
			this.node = node;
			this.game = game;
			exists = game.Statistics.UnlockedSpells.ContainsKey(node.InnerName);
		}

		public override void Tick()
		{
			base.Tick();

			recharge--;
			duration--;
			if (activated)
			{
				if (recharge < 0)
				{
					activated = false;
					SetColor(Color.White);
				}
				else if (duration > 0)
				{
					var sin = (float)Math.Sin(duration / 8f) * 0.2f + 0.2f;
					SetColor(Color.White + new Color(sin, sin, sin));
				}
			}
		}

		protected override void takeAction()
		{
			if (recharge < 0 && (node.Unlocked || exists && game.Statistics.UnlockedSpells[node.InnerName]))
			{
				if (game.Statistics.Mana >= node.Spell.ManaCost)
				{
					recharge = node.Spell.RechargeDuration;
					duration = node.Spell.Duration;

					game.World.LocalPlayer.Effects.Add(new Objects.Effects.EffectPart(game.World.LocalPlayer, node.Spell));
					game.Statistics.Mana -= node.Spell.ManaCost;

					activated = true;
					SetColor(new Color(0.5f, 0.5f, 0.5f));
				}
			}
		}
	}
}
