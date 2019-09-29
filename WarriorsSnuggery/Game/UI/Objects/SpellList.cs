using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Spells;

namespace WarriorsSnuggery.UI
{
	public class SpellList : PanelList
	{
		readonly ImageRenderable selector;
		public int CurrentSpell
		{
			get
			{
				return currentSpell;
			}
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
			selector = new ImageRenderable(TextureManager.Texture("UI_selector"));
		}

		public override void Render()
		{
			base.Render();
			selector.SetPosition(Container[currentSpell].Position + new CPos(0, -712, 0));
			selector.Render();
		}
	}
}
