namespace WarriorsSnuggery.Objects.Actors.Parts
{
	[Desc("Adds a weapon to the object.", "IMPORTANT NOTE: Currently, shroud is only supported for teams 0-9. If you use higher team values, the game will crash!")]
	public class RevealsShroudPartInfo : PartInfo
	{
		[Desc("Range of revealing shroud.", "Given in half terrain dimension. (2 = 1 terrain size)")]
		public readonly int Range = 0;

		[Desc("Interval in which the game should check for revealled shroud by this actor.")]
		public readonly int Interval = 0;

		public RevealsShroudPartInfo(PartInitSet set) : base(set) { }

		public override ActorPart Create(Actor self)
		{
			return new RevealsShroudPart(self, this);
		}
	}

	public class RevealsShroudPart : ActorPart, ITick, INoticeMove, ISaveLoadable
	{
		readonly RevealsShroudPartInfo info;
		int tick;
		bool firstActive;

		public int Range => info.Range;

		public RevealsShroudPart(Actor self, RevealsShroudPartInfo info) : base(self)
		{
			this.info = info;
			firstActive = true;
		}

		public void OnLoad(PartLoader loader)
		{
			foreach (var node in loader.GetNodes(typeof(RevealsShroudPart), info.InternalName))
			{
				if (node.Key == "Tick")
					tick = node.Convert<int>();
			}
		}

		public PartSaver OnSave()
		{
			var saver = new PartSaver(this, info.InternalName);

			saver.Add("Tick", tick, 0);

			return saver;
		}

		public void OnMove(CPos old, CPos speed)
		{
			if (tick < 0)
			{
				if (old.X / 512 - self.Position.X / 512 == 0 && old.Y / 512 - self.Position.Y / 512 == 0)
					return;

				self.World.ShroudLayer.RevealShroudCircular(self.World, self.Team, self.Position, info.Range);
				tick = info.Interval;
			}
		}

		public void Tick()
		{
			tick--;
			if (firstActive)
			{
				self.World.ShroudLayer.RevealShroudCircular(self.World, self.Team, self.Position, info.Range, true);
				firstActive = false;
			}
		}
	}
}
