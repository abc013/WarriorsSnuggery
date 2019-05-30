using WarriorsSnuggery.Objects.Parts;

namespace WarriorsSnuggery.Objects
{
	public class ActorType
	{
		public readonly int Height;
		
		public readonly PhysicsPartInfo Physics;
		public readonly PlayablePartInfo Playable;

		public readonly PartInfo[] PartInfos;

		public ActorType(int height, PhysicsPartInfo physics, PlayablePartInfo playable, PartInfo[] partInfos)
		{
			Height = height;

			Playable = playable;
			Physics = physics;
			PartInfos = partInfos;
		}
	}

}
