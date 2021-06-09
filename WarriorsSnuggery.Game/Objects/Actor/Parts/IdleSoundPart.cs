namespace WarriorsSnuggery.Objects.Actors.Parts
{
	[Desc("Plays a sound while the actor is alive.")]
	public class IdleSoundPartInfo : PartInfo
	{
		[Desc("Sound to use. This is required.")]
		public readonly SoundType Sound;

		public IdleSoundPartInfo(PartInitSet set) : base(set) { }

		public override ActorPart Create(Actor self)
		{
			return new IdleSoundPart(self, this);
		}
	}

	public class IdleSoundPart : ActorPart, INoticeMove, INoticeDispose
	{
		readonly Sound sound;

		public IdleSoundPart(Actor self, IdleSoundPartInfo info) : base(self)
		{
			sound = new Sound(info.Sound);
			sound.Play(self.Position, true);
		}

		public void OnMove(CPos old, CPos speed)
		{
			sound?.SetPosition(self.Position);
		}

		public void OnDispose()
		{
			sound?.Stop();
		}
	}
}
