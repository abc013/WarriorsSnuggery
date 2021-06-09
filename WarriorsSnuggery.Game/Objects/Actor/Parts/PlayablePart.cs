namespace WarriorsSnuggery.Objects.Actors.Parts
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

		[Desc("Determines an actor that is used when switching to another actor.", "When left empty, the switch to the next actor will be instant.")]
		public readonly string PlayerSwitchActor = string.Empty;

		public PlayablePartInfo(PartInitSet set) : base(set) { }

		public override ActorPart Create(Actor self)
		{
			return new PlayablePart(self, this);
		}
	}

	public class PlayablePart : ActorPart
	{
		readonly PlayablePartInfo info;

		public ActorType PlayerSwitchActor => string.IsNullOrEmpty(info.PlayerSwitchActor) ? null : ActorCreator.Types[info.PlayerSwitchActor];

		public PlayablePart(Actor self, PlayablePartInfo info) : base(self)
		{
			this.info = info;
		}
	}
}
