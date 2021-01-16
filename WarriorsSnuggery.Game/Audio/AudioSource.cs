using OpenTK.Audio.OpenAL;

namespace WarriorsSnuggery.Audio
{
	public abstract class AudioSource
	{
		public bool Used { get; private set; }

		protected readonly int Source;

		float volume;

		public AudioSource()
		{
			Source = AL.GenSource();
		}

		protected void Start()
		{
			Used = true;

			AL.SourcePlay(Source);
		}

		public void SetPitch(float pitch)
		{
			AL.Source(Source, ALSourcef.Pitch, pitch);
		}

		public void SetVolume(float volume, float master)
		{
			this.volume = volume;
			UpdateVolume(master);
		}

		public void UpdateVolume(float master)
		{
			AL.Source(Source, ALSourcef.Gain, volume * master);
		}

		public void SetPosition(Vector position)
		{
			AL.Source(Source, ALSource3f.Position, position.X, position.Y, position.Z);
		}

		public bool IsUsed()
		{
			Used = AL.GetSourceState(Source) == ALSourceState.Playing;

			if (!Used)
				ResetData();

			return Used;
		}

		public void Stop()
		{
			AL.SourceStop(Source);

			Used = false;

			ResetData();
		}

		protected virtual void ResetData()
		{
			volume = 1f;

			AL.Source(Source, ALSourcef.Gain, 1f);
			AL.Source(Source, ALSourcef.Pitch, 1f);
			AL.Source(Source, ALSourceb.Looping, false);
		}

		public void Pause(bool pause)
		{
			if (pause)
				AL.SourcePause(Source);
			else if (AL.GetSourceState(Source) == ALSourceState.Paused)
				AL.SourcePlay(Source);
		}

		public void Dispose()
		{
			AL.DeleteSource(Source);
		}
	}
}
