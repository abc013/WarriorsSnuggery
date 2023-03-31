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
	}

	public class RevealsShroudPart : ActorPart, ITick, INoticeMove, ISaveLoadable
	{
		readonly RevealsShroudPartInfo info;
		[Save("Tick"), DefaultValue(0)]
		int tick;
		bool firstActive;

		public int Range => info.Range;

		public RevealsShroudPart(Actor self, RevealsShroudPartInfo info) : base(self, info)
		{
			this.info = info;
			firstActive = true;
		}

		public void OnLoad(PartLoader loader)
		{
			loader.SetSaveFields(this);
		}

		public PartSaver OnSave()
		{
			var saver = new PartSaver(this);
			saver.AddSaveFields(this);

			return saver;
		}

		public void OnMove(CPos old, CPos speed)
		{
			if (tick < 0)
			{
				if (old.X / 512 - Self.Position.X / 512 == 0 && old.Y / 512 - Self.Position.Y / 512 == 0)
					return;

				Self.World.ShroudLayer.RevealShroudCircular(Self.World, Self.Team, Self.Position, info.Range);
				tick = info.Interval;
			}
		}

		public void Tick()
		{
			tick--;
			if (firstActive)
			{
				Self.World.ShroudLayer.RevealShroudCircular(Self.World, Self.Team, Self.Position, info.Range, true);
				firstActive = false;
			}
		}
	}
}
