using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Spells;

namespace WarriorsSnuggery.UI
{
	public class ActorList : PanelList
	{
		readonly ImageRenderable selector;
		public int CurrentActor
		{
			get
			{
				return currentActor;
			}
			set
			{
				currentActor = value;

				if (currentActor >= Container.Count)
					currentActor = 0;

				if (currentActor < 0)
					currentActor = Container.Count - 1;
			}
		}
		int currentActor;

		public ActorList(CPos pos, MPos bounds, MPos itemSize, PanelType type) : base(pos, bounds, itemSize, type)
		{
			selector = new ImageRenderable(TextureManager.Texture("UI_selector2"));
		}

		public override void Render()
		{
			base.Render();
			selector.SetPosition(Container[currentActor].Position + new CPos(-712, 0, 0));
			selector.Render();
		}
	}
}
