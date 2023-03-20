namespace WarriorsSnuggery.Objects.Actors.Parts
{
	[Desc("Attach this to the actor type to make it targetable by the player.")]
	public class TargetablePartInfo : PartInfo
	{
		[Desc("Up-left-corner of the selection box for possible targets.")]
		public readonly CPos TargetBoxCorner1 = new CPos(-256, 256, 0);

		[Desc("Down-right-corner of the selection box for possible targets.")]
		public readonly CPos TargetBoxCorner2 = new CPos(256, -256, 0);

		public TargetablePartInfo(PartInitSet set) : base(set) { }
	}

	public class TargetablePart : ActorPart
	{
		readonly TargetablePartInfo info;

		public TargetablePart(Actor self, TargetablePartInfo info) : base(self)
		{
			this.info = info;
		}

		public bool InTargetBox(CPos pos)
		{
			var diff = pos - self.Position;
			return diff.X > info.TargetBoxCorner1.X && diff.X < info.TargetBoxCorner2.X && diff.Y > -info.TargetBoxCorner1.Y && diff.Y < -info.TargetBoxCorner2.Y;
		}
	}
}
