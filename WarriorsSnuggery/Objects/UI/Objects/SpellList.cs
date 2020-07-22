using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Spells;

namespace WarriorsSnuggery.UI
{
	public class SpellList : PanelList
	{
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
			}
		}
		int currentSpell;

		public SpellList(CPos pos, MPos bounds, MPos itemSize, PanelType type) : base(pos, bounds, itemSize, type)
		{
			selector = new BatchObject(UITextureManager.Get("UI_selector1")[0], Color.White);
		}

		public void Update()
		{
			foreach (var content in Container)
				((SpellListItem)content).Update();
		}

		public override void Render()
		{
			base.Render();
			selector.SetPosition(Container[currentSpell].Position + new CPos(0, -688, 0));
			selector.PushToBatchRenderer();
		}
	}
}
