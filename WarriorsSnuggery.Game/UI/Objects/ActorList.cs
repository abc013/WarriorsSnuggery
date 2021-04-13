using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.UI
{
	public class ActorList : PanelList
	{
		readonly BatchObject selector;
		public int CurrentActor
		{
			get => currentActor;
			set
			{
				currentActor = value;

				if (currentActor >= Container.Count)
					currentActor = 0;

				if (currentActor < 0)
					currentActor = Container.Count - 1;

				selector.SetPosition(Container[currentActor].Position + new CPos(712, 0, 0));
			}
		}
		int currentActor;

		public ActorList(CPos pos, MPos bounds, MPos itemSize, PanelType type) : base(pos, bounds, itemSize, type)
		{
			selector = new BatchObject(UITextureManager.Get("UI_selector2")[0], Color.White);
		}

		public override void Render()
		{
			base.Render();
			selector.PushToBatchRenderer();
		}
	}
}
