namespace WarriorsSnuggery.Objects.Parts
{
	[Desc("Attach this to an actor to make it playable by the player.")]
	public class PlayablePartInfo : PartInfo
	{
		[Desc("When true, this actor is unlocked from the beginning of the Game.")]
		public readonly bool Unlocked;

		[Desc("Cost to unlock this actor.")]
		public readonly int UnlockCost;
		[Desc("Cost to change to this actor.")]
		public readonly int Cost;

		[Desc("Displayed name of the actor.")]
		public readonly string Name;
		[Desc("Description of the actor.")]
		public readonly string Description;

		public override ActorPart Create(Actor self)
		{
			return new PlayablePart(self, this);
		}

		public PlayablePartInfo(string internalName, MiniTextNode[] nodes) : base(internalName, nodes)
		{

		}
	}

	public class PlayablePart : ActorPart
	{
		public readonly PlayablePartInfo Info;

		public PlayablePart(Actor self, PlayablePartInfo info) : base(self)
		{
			Info = info;
		}
	}
}
