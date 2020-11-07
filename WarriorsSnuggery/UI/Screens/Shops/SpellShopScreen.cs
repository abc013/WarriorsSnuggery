using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Spells;

namespace WarriorsSnuggery.UI
{
	public class SpellShopScreen : Screen
	{
		readonly Game game;

		readonly MoneyDisplay money;

		readonly SpellNode[] tree;
		readonly List<SpellConnection> lines = new List<SpellConnection>();

		public SpellShopScreen(Game game) : base("Spell Shop")
		{
			this.game = game;
			Title.Position = new CPos(0, -4096, 0);

			Content.Add(new Button(new CPos(0, 6144, 0), "Resume", "wooden", () => { game.Pause(false); game.ScreenControl.ShowScreen(ScreenType.DEFAULT); }));

			Content.Add(new Panel(new CPos(0, 256, 0), new MPos(8 * 1024, 3 * 1024), PanelManager.Get("wooden")));

			money = new MoneyDisplay(game, new CPos(-(int)(WindowInfo.UnitWidth / 2 * 1024) + 1024, 7192, 0));

			var active = UITextureManager.Get("UI_activeConnection");
			var inactive = UITextureManager.Get("UI_inactiveConnection");
			tree = new SpellNode[SpellTreeLoader.SpellTree.Count];
			for (int i = 0; i < tree.Length; i++)
			{
				var origin = SpellTreeLoader.SpellTree[i];
				SpellNode spell = new SpellNode(origin.VisualPosition, origin, game, this);
				spell.CheckAvailability();
				tree[i] = spell;
				foreach (var connection in origin.Before)
				{
					if (connection == "")
						continue;

					var target = SpellTreeLoader.SpellTree.Find(s => s.InnerName == connection);
					var line = new SpellConnection(game, origin, target, active, inactive, 10);
					lines.Add(line);
				}
			}
		}

		public override void Render()
		{
			base.Render();

			foreach (var line in lines)
				line.Render();

			foreach (var panel in tree)
				panel.Render();

			money.Render();
		}

		public override void Tick()
		{
			base.Tick();

			foreach (var panel in tree)
				panel.Tick();

			money.Tick();
		}

		public override void KeyDown(Keys key, bool isControl, bool isShift, bool isAlt)
		{
			if (key == Keys.Escape)
				game.ChangeScreen(ScreenType.DEFAULT, false);
		}

		public void UpdateAvailability()
		{
			foreach (var node in tree)
				node.CheckAvailability();
		}

		public override void Hide()
		{
			foreach (var spell in tree)
				spell.DisableTooltip();

			game.ScreenControl.UpdateSpells();
		}
	}

	class SpellNode : Panel
	{
		readonly SpellTreeNode node;
		readonly Game game;
		readonly SpellShopScreen screen;

		readonly BatchSequence image;
		readonly Tooltip tooltip;
		bool mouseOnItem;
		bool available;

		public SpellNode(CPos position, SpellTreeNode node, Game game, SpellShopScreen screen) : base(position, new MPos((int)(1024 * 8 * MasterRenderer.PixelMultiplier), (int)(1024 * 8 * MasterRenderer.PixelMultiplier)), PanelManager.Get("stone"))
		{
			this.node = node;
			this.game = game;
			this.screen = screen;
			image = new BatchSequence(node.Textures, Color.White, node.Icon.Tick);
			image.SetPosition(position);

			tooltip = new Tooltip(position, node.Name + " : " + node.Cost, node.GetInformation(true));

			if (node.Unlocked || Program.IgnoreTech || game.Statistics.UnlockedSpells.Contains(node.InnerName))
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

			image.Tick();
		}

		public override void Render()
		{
			base.Render();

			image.SetColor(available ? Color.White : Color.Black);
			image.PushToBatchRenderer();
		}

		public void CheckAvailability()
		{
			available |= HighlightVisible;

			if (available)
				return;

			foreach (var before in node.Before)
			{
				if (string.IsNullOrWhiteSpace(before))
					continue;

				if (game.Statistics.UnlockedSpells.Contains(before))
					continue;

				foreach (var node in SpellTreeLoader.SpellTree)
				{
					if (node.InnerName == before && !node.Unlocked)
						return;
				}
			}

			available = true;
		}

		void checkMouse()
		{
			var mousePosition = MouseInput.WindowPosition;

			mouseOnItem = mousePosition.X > Position.X - 512 && mousePosition.X < Position.X + 512 && mousePosition.Y > Position.Y - 512 && mousePosition.Y < Position.Y + 512;

			if (mouseOnItem && !node.Unlocked && MouseInput.IsLeftClicked)
			{
				if (HighlightVisible)
					return;

				if (!available)
					return;

				if (game.Statistics.Money < node.Cost)
					return;

				game.Statistics.Money -= node.Cost;
				game.Statistics.UnlockedSpells.Add(node.InnerName);

				HighlightVisible = true;

				screen.UpdateAvailability();
			}
		}
	}

	class SpellConnection : IRenderable
	{
		public bool Active
		{
			set
			{
				renderables = value ? active : inactive;
			}
		}

		readonly Game game;

		readonly CPos originPos;
		readonly SpellTreeNode target;
		readonly CPos targetPos;

		readonly int renderabledistance;
		readonly BatchRenderable[] inactive;
		readonly BatchRenderable[] active;
		BatchRenderable[] renderables;
		readonly int tick;
		int curTick;
		int frame;

		public SpellConnection(Game game, SpellTreeNode origin, SpellTreeNode target, Texture[] active, Texture[] inactive, int tick)
		{
			this.game = game;
			originPos = origin.VisualPosition;
			this.target = target;
			targetPos = target.VisualPosition;
			this.active = new BatchRenderable[active.Length];
			for (int i = 0; i < active.Length; i++)
				this.active[i] = new BatchObject(active[i], Color.White);
			this.inactive = new BatchRenderable[inactive.Length];
			for (int i = 0; i < inactive.Length; i++)
				this.inactive[i] = new BatchObject(inactive[i], Color.White);
			this.tick = tick;

			renderabledistance = 1024 * active[0].Height / MasterRenderer.PixelSize;
			Active = false;
		}

		public void Render()
		{
			if (target.Unlocked || game.Statistics.UnlockedSpells.Contains(target.InnerName))
				Active = true;

			if (--curTick < 0)
			{
				if (--frame < 0)
					frame = renderables.Length - 1;

				curTick = tick;
			}

			var distance = (originPos - targetPos).FlatDist;
			var angle = (originPos - targetPos).FlatAngle;
			var fit = distance / renderabledistance;

			var curFrame = frame;
			for (int i = 0; i < fit; i++)
			{
				var renderable = renderables[curFrame];

				var posX = (int)(Math.Cos(angle) * i * renderabledistance);
				var posY = (int)(Math.Sin(angle) * i * renderabledistance);

				renderable.SetRotation(new VAngle(0, 0, -angle) + new VAngle(0, 0, 270));
				renderable.SetPosition(originPos + new CPos(posX, posY, 0));
				renderable.PushToBatchRenderer();

				curFrame--;
				if (curFrame < 0)
					curFrame = renderables.Length - 1;
			}
		}
	}
}
