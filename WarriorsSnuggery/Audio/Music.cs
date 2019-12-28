namespace WarriorsSnuggery.Audio
{
	public class Music
	{
		public readonly int Length;
		int length;
		bool paused;

		public bool Done
		{
			get { return length <= 0; }
			set { }
		}

		readonly AudioBuffer buffer;
		AudioSource source;

		public Music(string name)
		{
			buffer = AudioManager.GetBuffer(name);
			Length = buffer.Length;
		}

		public void Play()
		{
			length = Length;
			source = AudioController.Play(buffer, false, Settings.MusicVolume, false);
		}

		public void SetVolume()
		{
			if (source != null)
				source.SetVolume(Settings.MusicVolume);
		}

		public void Tick()
		{
			if (!paused)
				length--;
		}

		public void Pause(bool pause)
		{
			paused = pause;
			if (source != null)
				source.Pause(pause);
		}

		public void Stop()
		{
			if (source != null)
				source.Stop();
			source = null;
		}
	}
}
