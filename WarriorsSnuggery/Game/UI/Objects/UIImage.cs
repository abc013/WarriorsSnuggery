using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.UI
{
	public class UIImage : ITickRenderable
	{
		readonly BatchObject @object;

		public UIImage(CPos pos, BatchObject @object, float scale = 1f)
		{
			this.@object = @object;
			@object.SetPosition(pos);
			@object.SetScale(scale);
		}

		public void Render()
		{
			@object.PushToBatchRenderer();
		}

		public void Tick()
		{
		}
	}
}
