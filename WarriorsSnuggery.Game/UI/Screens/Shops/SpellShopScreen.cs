using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Collections.Generic;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Spells;
using WarriorsSnuggery.UI.Objects;

namespace WarriorsSnuggery.UI.Screens
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
			Title.Position = new UIPos(0, -4096);

			Add(new Button("Resume", "wooden", () => game.ShowScreen(ScreenType.DEFAULT, false)) { Position = new UIPos(0, 6144) });

			Add(new Panel(new UIPos(8 * 1024, 3 * 1024), "wooden") { Position = new UIPos(0, 256) });

			money = new MoneyDisplay(game) { Position = new UIPos(Left + 2048, Bottom - 1024) };

			var active = UISpriteManager.Get("UI_activeConnection");
			var inactive = UISpriteManager.Get("UI_inactiveConnection");
			tree = new SpellNode[SpellCasterCache.Types.Count];

			var i = 0;
			foreach (var origin in SpellCasterCache.Types.Values)
			{
				var spell = new SpellNode(origin, game, this) { Position = origin.VisualPosition };
				spell.CheckAvailability();
				tree[i++] = spell;

				foreach (var connection in origin.Before)
				{
					if (string.IsNullOrEmpty(connection))
						continue;

					var target = SpellCasterCache.Types[connection];
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
			base.KeyDown(key, isControl, isShift, isAlt);

			if (key == Keys.Escape)
				game.ShowScreen(ScreenType.DEFAULT, false);
		}

		public void UpdateAvailability()
		{
			foreach (var node in tree)
				node.CheckAvailability();

			game.ScreenControl.UpdateSpells();
		}

		public override void Hide()
		{
			foreach (var spell in tree)
				spell.DisableTooltip();

			game.ScreenControl.UpdateSpells();
		}
	}

	class SpellNode : Panel, ITick
	{
		public override UIPos Position
		{
			get => base.Position;
			set
			{
				base.Position = value;

				image.SetPosition(value);
			}
		}

		readonly SpellCasterType type;
		readonly Game game;
		readonly SpellShopScreen screen;

		readonly BatchSequence image;
		readonly Tooltip tooltip;
		bool available;
		bool unlocked;

		public SpellNode(SpellCasterType type, Game game, SpellShopScreen screen) : base(new UIPos((int)(1024 * 8 * Constants.PixelMultiplier), (int)(1024 * 8 * Constants.PixelMultiplier)), "stone", true)
		{
			this.type = type;
			this.game = game;
			this.screen = screen;
			image = new BatchSequence(type.Icon);

			tooltip = new Tooltip($"{type.Name}: {type.Cost}", type.GetDescription());

			unlocked = game.Player.HasSpellUnlocked(type);
			HighlightVisible = unlocked;
		}

		public virtual void DisableTooltip()
		{
			UIRenderer.DisableTooltip(tooltip);
		}

		public void Tick()
		{
			if (UIUtils.ContainsMouse(this))
			{
				// TODO: custom bounds 512, 512
				if (MouseInput.IsLeftClicked)
				{
					if (unlocked || !available)
						return;

					if (game.Player.Money < type.Cost)
						return;

					UIUtils.PlaySellSound();

					game.Player.Money -= type.Cost;
					game.Player.AddSpell(type);

					unlocked = true;
					HighlightVisible = true;

					screen.UpdateAvailability();

				}

				UIRenderer.SetTooltip(tooltip);
			}
			else
				UIRenderer.DisableTooltip(tooltip);

			image.Tick();
		}

		public override void Render()
		{
			base.Render();

			image.SetColor(available ? Color.White : Color.Black);
			image.Render();
		}

		public void CheckAvailability()
		{
			available = game.Player.IsSpellUnlockable(type);
		}
	}

	class SpellConnection : IRenderable
	{
		public bool Active
		{
			set => renderables = value ? active : inactive;
		}

		readonly Game game;

		readonly UIPos originPos;
		readonly SpellCasterType target;
		readonly UIPos targetPos;

		readonly int renderabledistance;
		readonly BatchRenderable[] inactive;
		readonly BatchRenderable[] active;
		BatchRenderable[] renderables;
		readonly int tick;
		int curTick;
		int frame;

		public SpellConnection(Game game, SpellCasterType origin, SpellCasterType target, Texture[] active, Texture[] inactive, int tick)
		{
			this.game = game;
			originPos = origin.VisualPosition;
			this.target = target;
			targetPos = target.VisualPosition;
			this.active = new BatchRenderable[active.Length];
			for (int i = 0; i < active.Length; i++)
				this.active[i] = new BatchObject(active[i]);
			this.inactive = new BatchRenderable[inactive.Length];
			for (int i = 0; i < inactive.Length; i++)
				this.inactive[i] = new BatchObject(inactive[i]);
			this.tick = tick;

			renderabledistance = 1024 * active[0].Height / Constants.PixelSize;
			Active = false;
		}

		public void Render()
		{
			if (game.Player.HasSpellUnlocked(target))
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

				var pos = CPos.FromFlatAngle(angle, i * renderabledistance);

				renderable.SetRotation(new VAngle(0, 0, -angle) + new VAngle(0, 0, 270));
				renderable.SetPosition(originPos + pos);
				renderable.Render();

				curFrame--;
				if (curFrame < 0)
					curFrame = renderables.Length - 1;
			}
		}
	}
}
