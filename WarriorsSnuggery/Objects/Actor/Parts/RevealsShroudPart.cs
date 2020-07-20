namespace WarriorsSnuggery.Objects.Parts
{
	[Desc("Adds a weapon to the object.", "IMPORTANT NOTE: Currently, shroud is only supported for teams 0-9. If you use higher team values, the game will crash!")]
	public class RevealsShroudPartInfo : PartInfo
	{
		[Desc("Range of revealing shroud.", "Given in half terrain dimension. (2 = 1 terrain size)")]
		public readonly int Range = 0;

		[Desc("Interval in which the game should check for revealled shroud by this actor.")]
		public readonly int Interval = 0;

		public override ActorPart Create(Actor self)
		{
			return new RevealsShroudPart(self, this);
		}

		public RevealsShroudPartInfo(string internalName, MiniTextNode[] nodes) : base(internalName, nodes) { }
	}

	public class RevealsShroudPart : ActorPart
	{
		readonly RevealsShroudPartInfo info;
		int tick;
		bool firstActive;

		public int Range
		{
			get { return info.Range; }
		}

		public RevealsShroudPart(Actor self, RevealsShroudPartInfo info) : base(self)
		{
			this.info = info;
			firstActive = true;
		}

		public override void OnMove(CPos old, CPos speed)
		{
			if (tick < 0)
			{
				// Use Rectangular as Circular is sill unperformant
				self.World.ShroudLayer.RevealShroudCircular(self.Team, (self.Position * new CPos(2, 2, 0)).ToMPos(), info.Range);
				tick = info.Interval;
			}
		}

		public override void Tick()
		{
			if (self.World.Game.Editor)
				return;

			tick--;
			if (firstActive)
			{
				self.World.ShroudLayer.RevealShroudCircular(self.Team, (self.Position * new CPos(2, 2, 0)).ToMPos(), info.Range, true);
				firstActive = false;
			}
		}
	}
}
