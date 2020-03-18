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
			source = AudioController.Play(buffer, false, Settings.MusicVolume, Vector.Zero, false);
		}

		public void SetVolume()
		{
			source?.UpdateVolume(Settings.MusicVolume * Settings.MasterVolume);
		}

		public void Tick()
		{
			if (!paused)
				length--;
		}

		public void Pause(bool pause)
		{
			paused = pause;
			source?.Pause(pause);
		}

		public void Stop()
		{
			source?.Stop();
			source = null;
		}
	}
}
