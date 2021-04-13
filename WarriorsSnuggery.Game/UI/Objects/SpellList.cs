using OpenTK.Windowing.GraphicsLibraryFramework;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Spells;

namespace WarriorsSnuggery.UI
{
	public class SpellList : PanelList
	{
		readonly Game game;

		readonly BatchObject selector;
		public int CurrentSpell
		{
			get => currentSpell;
			set
			{
				currentSpell = value;

				if (currentSpell >= SpellTreeLoader.SpellTree.Count)
					currentSpell = 0;

				if (currentSpell < 0)
					currentSpell = SpellTreeLoader.SpellTree.Count - 1;

				selector.SetPosition(Container[currentSpell].Position - new CPos(712, 0, 0));
			}
		}
		int currentSpell;

		public SpellList(Game game, CPos pos, MPos bounds, MPos itemSize, PanelType type) : base(pos, bounds, itemSize, type)
		{
			this.game = game;

			selector = new BatchObject(UITextureManager.Get("UI_selector1")[0], Color.White);

			addSpells();

			CurrentSpell = 0;
		}

		void addSpells()
		{
			int index = 0;
			foreach (var spell in SpellTreeLoader.SpellTree)
			{
				var item = new SpellListItem(new MPos(512, 512), spell, game.SpellManager.spellCasters[index++], game, true);
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

				if (!KeyInput.IsKeyDown(Keys.LeftControl) && MouseInput.IsRightClicked)
					game.SpellManager.Activate(CurrentSpell);
			}
		}
	}
}
