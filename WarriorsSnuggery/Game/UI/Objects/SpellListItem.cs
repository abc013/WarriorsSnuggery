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
		bool unlocked;
		int progress;

		public SpellListItem(CPos pos, MPos size, SpellTreeNode node, SpellCaster caster, Game game, bool showDesc) : base(pos, new BatchSequence(node.Textures, Color.White, node.Icon.Tick), size, node.Name, node.GetInformation(showDesc), null)
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
				progress = (int)(caster.RemainingDuration * -1024) + 512;
				tick++;
				var sin = (float)Math.Sin(tick / 8f) * 0.2f + 0.2f;
				SetColor(Color.White + new Color(sin, sin, sin));
			}
			else if (caster.Recharging)
			{
				progress = (int)(caster.RechargeProgress * -1024) + 512;
				SetColor(Color.White);
			}
			else
			{
				SetColor(unlocked ? Color.White : Color.Black);
			}
		}

		public void Update()
		{
			unlocked = caster.Unlocked();
		}

		public override void Render()
		{
			if (!caster.Ready)
				ColorManager.DrawRect(Position - new CPos(512, 512, 0), Position + new CPos(512, progress, 0), caster.Recharging ? new Color(0, 0, 0, 127) : new Color(255, 255, 255, 63));

			base.Render();
		}

		protected override void takeAction()
		{
			caster.Activate(game.World.LocalPlayer, MouseInput.GamePosition);
		}
	}
}
