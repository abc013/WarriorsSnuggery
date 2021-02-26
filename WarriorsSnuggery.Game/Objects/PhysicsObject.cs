using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Physics;

namespace WarriorsSnuggery.Objects
{
	public class PhysicsObject : PositionableObject
	{
		public SimplePhysics Physics { get; protected set; } = SimplePhysics.Empty;

		public PhysicsObject(CPos pos, BatchRenderable renderable) : base(pos, renderable)
		{

		}

		public override void Dispose()
		{
			foreach (var sector in Physics.Sectors)
				sector.Leave(this);

			base.Dispose();
		}
	}
}
