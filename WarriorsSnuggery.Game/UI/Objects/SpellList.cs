using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Spells;

namespace WarriorsSnuggery.UI.Objects
{
	public class SpellList : PanelList
	{
		readonly Game game;

		readonly BatchObject selector;
		readonly int spellCount;

		public override CPos Position
		{
			get => base.Position;
			set
			{
				base.Position = value;

				selector.SetPosition(Container[currentSpell].Position - new CPos(712, 0, 0));
			}
		}

		public int CurrentSpell
		{
			get => currentSpell;
			set
			{
				currentSpell = value;

				currentSpell %= spellCount;

				if (currentSpell < 0)
					currentSpell = spellCount - 1;

				selector.SetPosition(Container[currentSpell].Position - new CPos(712, 0, 0));
			}
		}
		int currentSpell;

		public SpellList(Game game, MPos bounds, MPos itemSize, string typeName) : this(game, bounds, itemSize, PanelManager.Get(typeName)) { }

		public SpellList(Game game, MPos bounds, MPos itemSize, PanelType type) : base(bounds, itemSize, type)
		{
			this.game = game;

			selector = new BatchObject(UISpriteManager.Get("UI_selector1")[0]);
			spellCount = SpellTreeLoader.SpellTree.Count;

			addSpells();
		}

		void addSpells()
		{
			int index = 0;
			foreach (var spell in SpellTreeLoader.SpellTree)
			{
				var item = new SpellListItem(game, ItemSize, spell, game.SpellManager.spellCasters[index++]);
				Add(item);
			}
		}

		public void Update()
		{
			foreach (var content in Container)
				((SpellListItem)content).Update();
		}

		public override void Render()
		{
			base.Render();

			selector.PushToBatchRenderer();
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
	}
}
