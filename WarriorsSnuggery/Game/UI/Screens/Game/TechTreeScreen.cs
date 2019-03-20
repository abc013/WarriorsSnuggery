using System;
using System.Collections.Generic;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI
{
	public class TechTreeScreen : Screen
	{
		readonly Game game;

		readonly Button back;

		readonly Panel @base;

		readonly GameObject money;
		readonly Text moneyText;
		int cashCooldown;
		int lastCash;

		readonly List<TechNode> tree = new List<TechNode>();

		public TechTreeScreen(Game game) : base("Tech Tree")
		{
			this.game = game;
			Title.Position = new CPos(0, -4096, 0);

			back = ButtonCreator.Create("wooden", new CPos(0, 6144, 0), "Resume", () => { game.ScreenControl.ShowScreen(ScreenType.DEFAULT); game.Pause(false); });
			@base = new Panel(new CPos(0, 1024, 0), new MPos(8192 / 64 * 3, 4096 / 64 * 3), 4, "UI_wood1", "UI_wood3", "UI_wood2");

			money = new GameObject(new CPos(-(int)(WindowInfo.UnitWidth / 2 * 1024) + 1024, 7192, 0), new ImageRenderable(TextureManager.Texture("UI_money")));
			moneyText = new Text(new CPos(-(int)(WindowInfo.UnitWidth / 2 * 1024) + 2048, 7192, 0), IFont.Papyrus24);
			moneyText.SetText(game.Statistics.Money);

			foreach (var e in TechTreeLoader.TechTree)
			{
				TechNode pan = new TechNode(new CPos(-4096, -2048, 0) + e.Position.ToCPos(), e, game);
				tree.Add(pan);
			}
		}

		public override void Render()
		{
			base.Render();
			@base.Render();

			back.Render();

			foreach(var panel in tree)
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

		public override void Dispose()
		{
			base.Dispose();
			@base.Dispose();

			back.Dispose();

			foreach (var panel in tree)
			{
				panel.Dispose();
			}
			tree.Clear();
		}
	}

	class TechNode : Panel
	{
		readonly ITechTreeNode node;
		readonly Game game;

		readonly Text onHover;
		readonly Text onHover2;
		bool mouseOnItem;

		public TechNode(CPos position, ITechTreeNode node, Game game) : base(position, new MPos(16, 16), 3, "UI_stone1", "UI_stone2", "UI_wood2")
		{
			this.node = node;
			this.game = game;
			onHover = new Text(position, IFont.Pixel16);
			onHover.SetText(node.Name + " : " + node.Cost);
			onHover.Visible = false;
			UIRenderer.RenderAfter(onHover);
			onHover2 = new Text(position + new CPos(0,712,0), IFont.Pixel16);
			if (node.Before.Length > 0 || node.Before[0].Trim() == "") // TODO does not work
			{
				onHover2.SetText("Pre: ");
				for (int i = 0; i < node.Before.Length; i++)
					onHover2.AddText((i != 0 ? ", " : "") + node.Before[i]);
			}
			onHover2.Visible = false;
			UIRenderer.RenderAfter(onHover2);

			if (node.Unlocked || game.Statistics.UnlockedNodes.ContainsKey(node.InnerName) && game.Statistics.UnlockedNodes[node.InnerName])
				HighlightVisible = true;
		}

		public override void Tick()
		{
			base.Tick();

			checkMouse();

			onHover.Visible = mouseOnItem;
			onHover2.Visible = mouseOnItem;
			if (mouseOnItem)
			{
				onHover.Position = Position;
				onHover2.Position = Position + new CPos(0, 712, 0);
			}
		}

		void checkMouse()
		{
			var mousePosition = MouseInput.WindowPosition;

			mouseOnItem = mousePosition.X > Position.X - 512 && mousePosition.X < Position.X + 512 && mousePosition.Y > Position.Y - 512 && mousePosition.Y < Position.Y + 512;

			if (mouseOnItem && !node.Unlocked && MouseInput.isLeftClicked)
			{
				if (game.Statistics.UnlockedNodes.ContainsKey(node.InnerName) && game.Statistics.UnlockedNodes[node.InnerName])
					return;

				var prerequisitesMet = true;

				foreach(var before in node.Before)
				{
					if (before.Trim() == "")
						continue;

					if (game.Statistics.UnlockedNodes.ContainsKey(before) && game.Statistics.UnlockedNodes[before])
						continue;

					prerequisitesMet = false;
					foreach (var node in TechTreeLoader.TechTree)
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
				if (game.Statistics.UnlockedNodes.ContainsKey(node.InnerName))
				{
					game.Statistics.UnlockedNodes[node.InnerName] = true;
				}
				else
				{
					game.Statistics.UnlockedNodes.Add(node.InnerName, true);
				}
			}
		}

		public override void Dispose()
		{
			base.Dispose();

			UIRenderer.RemoveRenderAfter(onHover);
			onHover.Dispose();
			UIRenderer.RemoveRenderAfter(onHover2);
			onHover2.Dispose();
		}
	}
}
