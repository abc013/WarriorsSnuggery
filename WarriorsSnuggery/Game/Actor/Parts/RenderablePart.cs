
namespace WarriorsSnuggery.Objects.Parts
{
	public abstract class RenderablePart : ActorPart
	{
		public RenderablePart(Actor self) : base(self)
		{

		}

		public abstract int FacingFromAngle(float angle);

		public abstract Graphics.GraphicsObject GetRenderable(ActorAction action, int facing);

		public abstract override void Render();
	}
}
