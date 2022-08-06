namespace WarriorsSnuggery.Objects.Actors.Parts
{
	[Desc("Basic information about the object.")]
	public class WorldPartInfo : PartInfo
	{
		[Desc("When true, the actor will show a damage text.")]
		public readonly bool ShowDamage;

		[Desc("When true, the actor will be able to trigger other objects.")]
		public readonly bool CanTrigger;

		[Desc("Determines whether this actor needs to be killed in order to win a match.")]
		public readonly bool KillForVictory = false;

		[Desc("Hides the actor when the cursor/player is behind it so the player can see more.")]
		public readonly bool Hideable;

		[Desc("Size of the visbility box.", "This is used to determine when to hide the actor after it is out of sight.")]
		public readonly MPos VisibilityBox = new MPos(512, 512);
		[Desc("offset of the visibility box.", "This is used to determine when to hide the actor after it is out of sight.")]
		public readonly CPos VisibilityBoxOffset = CPos.Zero;

		[Desc("Adds this actor the the selection list in the editor.")]
		public readonly bool ShowInEditor = true;

		[Desc("Amount of time to pass until the actor is fully spawned.", "If you want to have a spawn animation, you need to set this to the desired delay.")]
		public readonly int SpawnDelay = 0;
		[Desc("Amount of time to pass until the actor is fully disposed.", "If you want to have a despawn/death animation, you need to set this to the desired delay.")]
		public readonly int DespawnDelay = 0;

		public WorldPartInfo(PartInitSet set) : base(set) { }

		public override ActorPart Create(Actor self)
		{
			return new WorldPart(self, this);
		}
	}

	public class WorldPart : ActorPart
	{
		readonly WorldPartInfo info;

		public MPos VisibilityBox => info.VisibilityBox;
		public CPos VisibilityBoxOffset => info.VisibilityBoxOffset;

		public bool ShowDamage => info.ShowDamage;

		public bool CanTrigger => info.CanTrigger;
		public bool KillForVictory => info.KillForVictory;

		public bool Hideable => info.Hideable;

		public int SpawnDelay => info.SpawnDelay;

		public int DespawnDelay => info.DespawnDelay;

		public WorldPart(Actor self, WorldPartInfo info) : base(self)
		{
			this.info = info;
		}
	}
}
