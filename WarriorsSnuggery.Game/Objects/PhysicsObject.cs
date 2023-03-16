using WarriorsSnuggery.Physics;

namespace WarriorsSnuggery.Objects
{
	public abstract class PhysicsObject : PositionableObject
	{
		public SimplePhysics Physics { get; protected set; } = SimplePhysics.Empty;

		public PhysicsObject() : base(null) { }

		public override void Dispose()
		{
			Physics.RemoveSectors();

			base.Dispose();
		}
	}
}
