using WarriorsSnuggery.Objects.Parts;

namespace WarriorsSnuggery.Objects
{
	public class ActorType
	{
		public readonly PhysicsPartInfo Physics;
		public readonly PlayablePartInfo Playable;

		public readonly PartInfo[] PartInfos;

		public ActorType(PhysicsPartInfo physics, PlayablePartInfo playable, PartInfo[] partInfos)
		{
			Playable = playable;
			Physics = physics;
			PartInfos = partInfos;
		}
	}

}
