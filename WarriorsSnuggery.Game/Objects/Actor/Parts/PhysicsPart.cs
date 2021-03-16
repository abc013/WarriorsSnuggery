using WarriorsSnuggery.Physics;

namespace WarriorsSnuggery.Objects.Parts
{
	[Desc("Attach this to an actor so it can collide with other actors. For further information, please look for SimplePhysics.")]
	public class PhysicsPartInfo : PartInfo
	{
		[Desc("Type of physics to use.")]
		public readonly SimplePhysicsType Type;

		public PhysicsPartInfo(PartInitSet set) : base(set) { }

		public override ActorPart Create(Actor self)
		{
			return new PhysicsPart(self, this);
		}
	}

	public class PhysicsPart : ActorPart
	{
		public PhysicsPart(Actor self, PhysicsPartInfo info) : base(self)
		{
			
		}
	}
}
