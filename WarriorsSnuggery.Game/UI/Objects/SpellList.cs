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

		public SpellList(Game game, MPos bounds, MPos itemSize, string typeName) : this(game, bounds, itemSize, PanelCache.Types[typeName]) { }

		public SpellList(Game game, MPos bounds, MPos itemSize, PanelType type) : base(bounds, itemSize, type, false)
		{
			this.game = game;

			spellCount = SpellCasterCache.Types.Count;

			addSpells();

			CurrentSpell = 0;
		}

		void addSpells()
		{
			int index = 0;
			foreach (var spell in SpellCasterCache.Types)
				Add(new SpellListItem(game, ItemSize, spell, game.SpellManager.Casters[index++]));
		}

		public void Update()
		{
			foreach (var content in Container)
				((SpellListItem)content).Update();
		}

		public override void Tick()
		{
			base.Tick();

			if (!KeyInput.IsKeyDown(Keys.LeftShift))
			{
				CurrentSpell += MouseInput.WheelState;
				if (KeyInput.IsKeyDown(Settings.KeyDictionary["Activate"]) || !KeyInput.IsKeyDown(Keys.LeftControl) && MouseInput.IsRightClicked)
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

			int tick;
			bool unlocked;
			float progress;
			int graphicProgress;

			public SpellListItem(Game game, MPos size, SpellCasterType node, SpellCaster caster) : base(new BatchSequence(node.Icon), size, node.Name, node.GetInformation(true), null)
			{
				this.caster = caster;
				this.game = game;
				unlocked = caster.Unlocked();

				manaCost = node.ManaCost;
			}

			public override void Tick()
			{
				base.Tick();

				if (caster.Activated)
				{
					progress = caster.RemainingDuration;
					graphicProgress = (int)(progress * -(Bounds.Y * 2)) + Bounds.Y;
					tick++;
					var sin = MathF.Sin(tick / 8f) * 0.2f + 0.2f;
					SetColor(Color.White + new Color(sin, sin, sin));
				}
				else if (caster.Recharging)
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

			public void Update()
			{
				unlocked = caster.Unlocked();
			}

			public override void Render()
			{
				if (!caster.Ready)
				{
					var color = caster.Recharging ? new Color(0, 0, 0, 127) : new Color(255, 255, 255, 63);
					var pointA = Position - new CPos(Bounds.X, Bounds.Y, 0);
					var pointB = Position + new CPos(Bounds.X, graphicProgress, 0);
					ColorManager.DrawRect(pointA, pointB, color);

					if (!caster.Recharging)
					{
						color = new Color(1f, 1f, 0, 1f - (progress * progress));
						pointB = Position + new CPos(Bounds.X, Bounds.Y, 0);
						ColorManager.DrawFilledLineRect(pointA, pointB, 32, color);
					}
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
