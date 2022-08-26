using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Physics;

namespace WarriorsSnuggery.Objects
{
	public class PhysicsObject : PositionableObject
	{
		public SimplePhysics Physics { get; protected set; } = SimplePhysics.Empty;

		public PhysicsObject(BatchRenderable renderable = null) : base(renderable) { }

		public override void Dispose()
		{
			Physics.RemoveSectors();

			base.Dispose();
		}
	}
}
