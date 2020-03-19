using OpenTK.Audio.OpenAL;

namespace WarriorsSnuggery.Audio
{
	public class AudioSource
	{
		public AudioBuffer Buffer { get; private set; }
		public bool Used;

		uint source;

		float volume;

		public AudioSource()
		{
			AL.GenSource(out source);
		}

		public void Start(AudioBuffer buffer, bool loops)
		{
			Used = true;
			Buffer = buffer;

			AL.BindBufferToSource(source, buffer.GetID());
			AL.Source(source, ALSourceb.Looping, loops);
			AL.SourcePlay(source);
		}

		public void SetPitch(float pitch)
		{
			AL.Source(source, ALSourcef.Pitch, pitch);
		}

		public void SetVolume(float volume, float master)
		{
			this.volume = volume;
			UpdateVolume(master);
		}

		public void UpdateVolume(float master)
		{
			AL.Source(source, ALSourcef.Gain, volume * master);
		}

		public void SetPosition(Vector position)
		{
			AL.Source(source, ALSource3f.Position, position.X, position.Y, position.Z);
		}

		public bool CheckUsed()
		{
			Used = AL.GetSourceState(source) == ALSourceState.Playing;
			return Used;
		}

		public void Stop()
		{
			AL.SourceStop(source);

			Used = false;
			Buffer = null;

			volume = 1f;

			AL.Source(source, ALSourcef.Gain, 1f);
			AL.Source(source, ALSourcef.Pitch, 1f);
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
