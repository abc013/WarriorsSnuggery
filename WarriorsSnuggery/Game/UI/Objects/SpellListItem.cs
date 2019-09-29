using System;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Spells;

namespace WarriorsSnuggery.UI
{
	public class SpellListItem : PanelItem
	{
		readonly SpellCaster caster;
		readonly Game game;

		int tick;
		readonly bool unlocked;

		public SpellListItem(CPos pos, MPos size, SpellTreeNode node, SpellCaster caster, Game game, bool showDesc) : base(pos, new IImageSequenceRenderable(node.Images, node.Icon.Tick), size, node.Name, node.getInformation(showDesc), null)
		{
			this.caster = caster;
			this.game = game;
			unlocked = caster.Unlocked();
		}

		public override void Tick()
		{
			base.Tick();

			if (caster.Activated)
			{
				tick++;
				var sin = (float)Math.Sin(tick / 8f) * 0.2f + 0.2f;
				SetColor(Color.White + new Color(sin, sin, sin));
			}
			else if (caster.Recharging)
			{
				SetColor(Color.Grey);
			}
			else
			{
				SetColor(unlocked ? Color.White : Color.Black);
			}
		}

		protected override void takeAction()
		{
			caster.Activate(game.World.LocalPlayer, MouseInput.GamePosition);
		}
	}
}
