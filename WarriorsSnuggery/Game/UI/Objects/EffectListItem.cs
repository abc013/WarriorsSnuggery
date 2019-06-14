using System;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI
{
	public class EffectListItem : PanelItem
	{
		readonly ITechTreeNode node;
		readonly Game game;

		int recharge;
		int duration;
		bool activated;
		readonly bool exists;

		public EffectListItem(CPos pos, MPos size, ITechTreeNode node, Game game) : base(pos, node.Name, new ImageRenderable(TextureManager.Texture(node.Icon)), size, null)
		{
			this.node = node;
			this.game = game;
			exists = game.Statistics.UnlockedNodes.ContainsKey(node.InnerName);
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
			if (recharge < 0 && (node.Unlocked || exists && game.Statistics.UnlockedNodes[node.InnerName]))
			{
				if (game.Statistics.Mana >= node.Effect.ManaCost)
				{
					game.World.LocalPlayer.Effects.Add(new Objects.Effects.EffectPart(game.World.LocalPlayer, node.Effect));
					game.Statistics.Mana -= node.Cost;
					recharge = node.Effect.RechargeDuration;
					duration = node.Effect.Duration;
					activated = true;
					SetColor(new Color(0.5f, 0.5f, 0.5f));
				}
			}
		}
	}
}
