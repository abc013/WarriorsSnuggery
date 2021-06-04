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

		public SpellList(Game game, MPos bounds, MPos itemSize, string typeName) : this(game, bounds, itemSize, PanelManager.Get(typeName)) { }

		public SpellList(Game game, MPos bounds, MPos itemSize, PanelType type) : base(bounds, itemSize, type, false)
		{
			this.game = game;

			spellCount = SpellTreeLoader.SpellTree.Count;

			addSpells();
		}

		void addSpells()
		{
			int index = 0;
			foreach (var spell in SpellTreeLoader.SpellTree)
				Add(new SpellListItem(game, ItemSize, spell, game.SpellManager.spellCasters[index++]));
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
			int progress;

			public SpellListItem(Game game, MPos size, SpellTreeNode node, SpellCaster caster) : base(new BatchSequence(node.Icon), size, node.Name, node.GetInformation(true), null)
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
					progress = (int)(caster.RemainingDuration * -1024) + 512;
					tick++;
					var sin = MathF.Sin(tick / 8f) * 0.2f + 0.2f;
					SetColor(Color.White + new Color(sin, sin, sin));
				}
				else if (caster.Recharging)
				{
					progress = (int)(caster.RechargeProgress * -1024) + 512;
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
					ColorManager.DrawRect(Position - new CPos(512, 512, 0), Position + new CPos(512, progress, 0), caster.Recharging ? new Color(0, 0, 0, 127) : new Color(255, 255, 255, 63));

				base.Render();
			}

			protected override void takeAction()
			{
				caster.Activate(game.World.LocalPlayer);
			}
		}
	}
}
