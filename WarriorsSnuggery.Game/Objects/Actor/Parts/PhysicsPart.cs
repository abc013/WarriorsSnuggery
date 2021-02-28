using System.Collections.Generic;
using WarriorsSnuggery.Physics;

namespace WarriorsSnuggery.Objects.Parts
{
	[Desc("Attach this to an actor so it can collide with other actors. For further information, please look for SimplePhysics.")]
	public class PhysicsPartInfo : PartInfo
	{
		public readonly SimplePhysicsType Type;

		public override ActorPart Create(Actor self)
		{
			return new PhysicsPart(self, this);
		}

		public PhysicsPartInfo(string internalName, List<MiniTextNode> nodes) : base(internalName, new List<MiniTextNode>())
		{
			Type = new SimplePhysicsType(nodes);
		}
	}

	public class PhysicsPart : ActorPart
	{
		public PhysicsPart(Actor self, PhysicsPartInfo info) : base(self)
		{
			
		}
	}
}
