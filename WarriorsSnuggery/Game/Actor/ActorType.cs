using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.Objects
{
	public class ActorType
	{
		public readonly TextureInfo Idle; //TODO: replace through separate class
		public readonly int IdleFacings;
		public readonly TextureInfo Walk;
		public readonly int WalkFacings;
		public readonly TextureInfo Attack;
		public readonly int AttackFacings;
		public readonly TextureInfo Shadow;

		public readonly CPos Offset;
		public readonly int Height;
		
		public readonly Parts.PhysicsPartInfo Physics;
		public readonly Parts.PlayablePartInfo Playable;

		public readonly PartInfo[] PartInfos;

		public ActorType(TextureInfo idle, int idleFacings, TextureInfo walk, int walkFacings, TextureInfo attack, int attackFacings, TextureInfo shadow, CPos offset, int height, Parts.PhysicsPartInfo physics, Parts.PlayablePartInfo playable, PartInfo[] partInfos)
		{
			Idle = idle;
			IdleFacings = idleFacings;
			Walk = walk;
			WalkFacings = walkFacings;
			Attack = attack;
			AttackFacings = attackFacings;
			Shadow = shadow;

			Offset = offset;
			Height = height;

			Playable = playable;
			Physics = physics;
			PartInfos = partInfos;
		}
	}

}
