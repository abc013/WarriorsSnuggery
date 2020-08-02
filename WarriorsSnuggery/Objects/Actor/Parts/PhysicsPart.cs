using WarriorsSnuggery.Physics;

namespace WarriorsSnuggery.Objects.Parts
{
	[Desc("Attach this to an actor so it can collide with other actors.")]
	public class PhysicsPartInfo : PartInfo
	{
		[Desc("Size of the collision field.", "Z is used for height boundary.")]
		public readonly CPos Size;
		[Desc("Shape of the collision field.", "Possible: CIRCLE, RECTANGLE, LINE_HORIZONTAL, LINE_VERTICAL, NONE")]
		public readonly Shape Shape;

		public override ActorPart Create(Actor self)
		{
			return new PhysicsPart(self, this);
		}

		public PhysicsPartInfo(string internalName, MiniTextNode[] nodes) : base(internalName, nodes)
		{

		}
	}

	public class PhysicsPart : ActorPart
	{
		readonly PhysicsPartInfo info;

		public Shape Shape => info.Shape;

		public int Height => info.Size.Z;

		public CPos Size => info.Size;

		public PhysicsPart(Actor self, PhysicsPartInfo info) : base(self)
		{
			this.info = info;
		}
	}
}
