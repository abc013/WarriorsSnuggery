using System.Collections.Generic;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;
using WarriorsSnuggery.Spells;

namespace WarriorsSnuggery.UI
{
	public class SpellTreeScreen : Screen
	{
		readonly Game game;

		readonly Button back;

		readonly Panel @base;

		readonly ImageRenderable money;
		readonly TextLine moneyText;
		int cashCooldown;
		int lastCash;

		readonly SpellNode[] tree;
		readonly List<CPos[]> lines = new List<CPos[]>();

		public SpellTreeScreen(Game game) : base("Spell Tree")
		{
			this.game = game;
			Title.Position = new CPos(0, -4096, 0);

			back = ButtonCreator.Create("wooden", new CPos(0, 6144, 0), "Resume", () => { game.Pause(false); game.ScreenControl.ShowScreen(ScreenType.DEFAULT); });
			@base = new Panel(new CPos(0, 1024, 0), new MPos(8192 / 64 * 3, 4096 / 64 * 3), PanelManager.GetType("wooden"));

			money = new ImageRenderable(TextureManager.Texture("UI_money"));
			money.SetPosition(new CPos(-(int)(WindowInfo.UnitWidth / 2 * 1024) + 1024, 7192, 0));

			moneyText = new TextLine(new CPos(-(int)(WindowInfo.UnitWidth / 2 * 1024) + 2048, 7192, 0), IFont.Papyrus24);
			moneyText.SetText(game.Statistics.Money);

			tree = new SpellNode[SpellTreeLoader.SpellTree.Count];
			for (int i = 0; i < tree.Length; i++)
			{
				var e = SpellTreeLoader.SpellTree[i];
				var position = new CPos(-4096, -2048, 0) + e.Position.ToCPos();
				SpellNode spell = new SpellNode(position, e, game);
				tree[i] = spell;
				foreach (var connection in e.Before)
				{
					if (connection == "")
						continue;

					var positionTo = new CPos(-4096, -2048, 0) + SpellTreeLoader.SpellTree.Find(s => s.InnerName == connection).Position.ToCPos();
					lines.Add(new[] { position, positionTo });
				}
			}
		}

		public override void Render()
		{
			base.Render();
			@base.Render();

			back.Render();

			foreach (var line in lines)
			{
				ColorManager.DrawLine(line[0], line[1], Color.Green);
			}
			foreach (var panel in tree)
			{
				panel.Render();
			}

			money.Render();
			moneyText.Render();
		}

		public override void Tick()
		{
			base.Tick();
			@base.Tick();

			back.Tick();

			foreach (var panel in tree)
			{
				panel.Tick();
			}

			if (KeyInput.IsKeyDown("escape", 10))
			{
				game.Pause(false);
				game.ChangeScreen(ScreenType.DEFAULT);
			}

			if (lastCash != game.Statistics.Money)
			{
				lastCash = game.Statistics.Money;
				moneyText.SetText(game.Statistics.Money);
				cashCooldown = 10;
			}
			if (cashCooldown-- > 0)
				moneyText.Scale = (cashCooldown / 10f) + 1f;
		}

		public override void Hide()
		{
			foreach (var spell in tree)
				spell.DisableTooltip();
		}

		public override void Dispose()
		{
			base.Dispose();
			@base.Dispose();

			back.Dispose();

			foreach (var panel in tree)
				panel.Dispose();
			moneyText.Dispose();
		}
	}

	class SpellNode : Panel
	{
		readonly SpellTreeNode node;
		readonly Game game;

		readonly IImageSequenceRenderable image;
		readonly Tooltip tooltip;
		bool mouseOnItem;

		public SpellNode(CPos position, SpellTreeNode node, Game game) : base(position, new MPos(16, 16), PanelManager.GetType("stone"))
		{
			this.node = node;
			this.game = game;
			image = new IImageSequenceRenderable(node.Images, node.Icon.Tick);
			image.SetPosition(position);

			tooltip = new Tooltip(position, node.Name + " : " + node.Cost, node.getInformation(true));

			if (node.Unlocked || game.Statistics.UnlockedSpells.ContainsKey(node.InnerName) && game.Statistics.UnlockedSpells[node.InnerName])
				HighlightVisible = true;
		}

		public virtual void DisableTooltip()
		{
			UIRenderer.DisableTooltip(tooltip);
		}

		public override void Tick()
		{
			base.Tick();

			checkMouse();

			if (mouseOnItem)
				UIRenderer.SetTooltip(tooltip);
			else
				UIRenderer.DisableTooltip(tooltip);
		}

		public override void Render()
		{
			base.Render();

			image.Render();
		}

		void checkMouse()
		{
			var mousePosition = MouseInput.WindowPosition;

			mouseOnItem = mousePosition.X > Position.X - 512 && mousePosition.X < Position.X + 512 && mousePosition.Y > Position.Y - 512 && mousePosition.Y < Position.Y + 512;

			if (mouseOnItem && !node.Unlocked && MouseInput.IsLeftClicked)
			{
				if (game.Statistics.UnlockedSpells.ContainsKey(node.InnerName) && game.Statistics.UnlockedSpells[node.InnerName])
					return;

				var prerequisitesMet = true;

				foreach (var before in node.Before)
				{
					if (before.Trim() == "")
						continue;

					if (game.Statistics.UnlockedSpells.ContainsKey(before) && game.Statistics.UnlockedSpells[before])
						continue;

					prerequisitesMet = false;
					foreach (var node in SpellTreeLoader.SpellTree)
					{
						if (node.InnerName == before)
						{
							prerequisitesMet = node.Unlocked;
							continue;
						}
					}
				}

				if (!prerequisitesMet)
					return;

				if (game.Statistics.Money < node.Cost)
					return;

				game.Statistics.Money -= node.Cost;
				HighlightVisible = true;
				if (game.Statistics.UnlockedSpells.ContainsKey(node.InnerName))
				{
					game.Statistics.UnlockedSpells[node.InnerName] = true;
				}
				else
				{
					game.Statistics.UnlockedSpells.Add(node.InnerName, true);
				}
			}
		}

		public override void Dispose()
		{
			tooltip.Dispose();

			base.Dispose();
		}
	}
}
