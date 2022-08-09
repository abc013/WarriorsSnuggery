using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Spells;

namespace WarriorsSnuggery.UI.Objects
{
	public class SpellList : PanelList
	{
		readonly Game game;

		readonly int spellCount;

		public int CurrentSpell
		{
			get => currentSpell;
			set
			{
				currentSpell = value;

				currentSpell %= spellCount;

				if (currentSpell < 0)
					currentSpell = spellCount - 1;

				SelectedPos = (currentSpell % Size.X, currentSpell / Size.X);
			}
		}
		int currentSpell;

		public SpellList(Game game, UIPos bounds, UIPos itemSize, string typeName) : this(game, bounds, itemSize, PanelCache.Types[typeName]) { }

		public SpellList(Game game, UIPos bounds, UIPos itemSize, PanelType type) : base(bounds, itemSize, type, false)
		{
			this.game = game;

			spellCount = SpellCasterCache.Types.Count;

			addSpells();

			CurrentSpell = 0;
		}

		void addSpells()
		{
			foreach (var caster in game.SpellManager.Casters)
				Add(new SpellListItem(game, ItemSize, caster));
		}

		public void Update()
		{
			Container.Clear();
			addSpells();
		}

		public override void Tick()
		{
			base.Tick();

			if (!KeyInput.IsKeyDown(Keys.LeftShift))
			{
				CurrentSpell += MouseInput.WheelState;
				if (KeyInput.IsKeyDown(Settings.GetKey("Activate")) || !KeyInput.IsKeyDown(Keys.LeftControl) && MouseInput.IsRightClicked)
					game.SpellManager.Activate(CurrentSpell);

				for (int i = 0; i < Math.Max(spellCount, 10); i++)
				{
					if (KeyInput.IsKeyDown(Keys.D0 + i))
						game.SpellManager.Activate((i + 9) % 10);
				}
			}
		}

		class SpellListItem : PanelListItem
		{
			static readonly Color inactive = new Color(0.6f, 0.6f, 0.6f);
			static readonly Color disabled = new Color(0f, 0f, 0f, 0.3f);

			readonly SpellCaster caster;
			readonly Game game;

			readonly int manaCost;
			readonly bool unlocked;

			int tick;
			float progress;
			int graphicProgress;

			public SpellListItem(Game game, UIPos size, SpellCaster caster) : base(new BatchSequence(caster.Type.Icon), size, (caster.Unlocked() ? Color.White : Color.Red) + caster.Type.Name, caster.Unlocked() ? caster.Type.GetDescription() : new[] { new Color(128, 0, 0) + "Unlock cost: " + caster.Type.Cost }, null)
			{
				this.caster = caster;
				this.game = game;
				unlocked = caster.Unlocked();

				manaCost = caster.Type.ManaCost;
			}

			public override void Tick()
			{
				base.Tick();

				if (caster.State == SpellCasterState.ACTIVE)
				{
					progress = caster.RemainingDuration;
					graphicProgress = (int)(progress * -(Bounds.Y * 2)) + Bounds.Y;
					tick++;
					var sin = MathF.Sin(tick / 8f) * 0.2f + 0.2f;
					SetColor(Color.White + new Color(sin, sin, sin));
				}
				else if (caster.State == SpellCasterState.RECHARGING)
				{
					progress = caster.RechargeProgress;
					graphicProgress = (int)(progress * -(Bounds.Y * 2)) + Bounds.Y;
					SetColor(inactive);
				}
				else
				{
					if (unlocked)
					{
						var enoughMana = game.Stats.Mana - manaCost > 0;

						SetColor(enoughMana ? Color.White : inactive);
						return;
					}
					SetColor(disabled);
				}
			}

			public override void Render()
			{
				if (caster.State == SpellCasterState.RECHARGING || caster.State == SpellCasterState.ACTIVE)
				{
					var recharging = caster.State == SpellCasterState.RECHARGING;

					var pointA = Position - new UIPos(Bounds.X, Bounds.Y);
					var pointB = Position + new UIPos(Bounds.X, graphicProgress);

					var color = recharging ? new Color(0, 0, 0, 127) : new Color(255, 255, 255, 63);
					ColorManager.DrawRect(pointA, pointB, color);
				}

				if (caster.State == SpellCasterState.SLEEPING || caster.State == SpellCasterState.ACTIVE)
				{
					var sleeping = caster.State == SpellCasterState.SLEEPING;

					var pointA = Position - new CPos(Bounds.X, Bounds.Y, 0);
					var pointB = Position + new CPos(Bounds.X, Bounds.Y, 0);

					var glowRadius = (int)(MathF.Sin(Window.GlobalTick / (sleeping ? 32f : 8f)) * 32) + 64;
					ColorManager.DrawGlowingFilledLineRect(pointA, pointB, 32, (sleeping ? new Color(0.5f, 0.5f, 1f, 1f) : new Color(1f, 0.5f, 0.5f, 1f)), glowRadius, 4);
					ColorManager.DrawFilledLineRect(pointA, pointB, 16, new Color(1f, 1f, 1f, 0.5f));
				}

				base.Render();
			}

			protected override void takeAction()
			{
				caster.Activate(game.World.LocalPlayer);
			}
		}
	}
}
