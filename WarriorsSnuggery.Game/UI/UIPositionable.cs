using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.UI
{
	public abstract class UIPositionable : IDebugRenderable
	{
		public virtual UIPos Position { get => position; set => position = value; }
		UIPos position;

		public virtual UIPos Bounds { get; protected set; }
		public UIPos SelectableBounds { get; protected set; }

		public virtual void DebugRender()
		{
			if (!Bounds.IsEmpty())
				ColorManager.DrawLineQuad(position, Bounds, Color.Red);

			if (!SelectableBounds.IsEmpty())
				ColorManager.DrawLineQuad(position, SelectableBounds, Color.Blue);
		}
	}
}
