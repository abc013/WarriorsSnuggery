using OpenTK.Audio.OpenAL;

namespace WarriorsSnuggery.Audio
{
	public class AudioSource
	{
		public AudioBuffer Buffer { get; private set; }
		public bool Used;

		uint source;

		public AudioSource()
		{
			AL.GenSource(out source);
		}

		public void Start(AudioBuffer buffer, float volume, bool loops)
		{
			Used = true;
			Buffer = buffer;

			AL.BindBufferToSource(source, buffer.GetID());
			AL.Source(source, ALSourcef.Gain, volume * Settings.MasterVolume);
			AL.Source(source, ALSourceb.Looping, loops);
			AL.SourcePlay(source);
		}

		public void SetVolume(float volume)
		{
			AL.Source(source, ALSourcef.Gain, volume * Settings.MasterVolume);
		}

		public void CheckUsed()
		{
			Used = AL.GetSourceState(source) == ALSourceState.Playing;
		}

		public void Stop()
		{
			AL.SourceStop(source);

			Used = false;
			Buffer = null;
			AL.Source(source, ALSourcef.Gain, 1f);
			AL.Source(source, ALSourceb.Looping, false);
		}

		public void Pause(bool pause)
		{
			if (pause)
				AL.SourcePause(source);
			else if (AL.GetSourceState(source) == ALSourceState.Paused)
				AL.SourcePlay(source);
		}

		public void Dispose()
		{
			AL.DeleteSource(ref source);
		}
	}
}
