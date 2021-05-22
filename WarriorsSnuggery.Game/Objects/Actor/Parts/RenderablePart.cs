
namespace WarriorsSnuggery.Objects.Actors.Parts
{
	public abstract class RenderablePart : ActorPart, IRenderable
	{
		public RenderablePart(Actor self) : base(self) { }

		public abstract int FacingFromAngle(float angle);

		public abstract Graphics.BatchRenderable GetRenderable(ActorAction action, int facing);

		public abstract void Render();

		public abstract void SetColor(Color color);
	}
}
