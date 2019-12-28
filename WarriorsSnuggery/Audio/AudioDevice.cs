using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

namespace WarriorsSnuggery.Audio
{
	public class AudioDevice
	{
		public readonly AudioSource[] Sources;
		public readonly AudioSource[] GameSources;
		readonly AudioContext context;
		readonly bool initialized;

		public AudioDevice()
		{
			var device = AudioContext.DefaultDevice;

			if (AudioContext.DefaultDevice == null)
				return;

			context = new AudioContext(device);
			context.MakeCurrent();
			var error = AL.GetError();
			if (error != ALError.NoError)
				throw new FailingSoundDeviceException(string.Format("Failed to open audio device. Error code: {0}.", error));

			initialized = true;
			Sources = new AudioSource[4];
			for (int i = 0; i < 4; i++)
				Sources[i] = new AudioSource();

			GameSources = new AudioSource[12];
			for (int i = 0; i < 12; i++)
				GameSources[i] = new AudioSource();
		}

		public AudioSource Play(AudioBuffer buffer, bool inGame, float volume, bool loops)
		{
			if (!initialized)
				return null;

			if (!inGame)
			{
				foreach (var source in Sources)
				{
					source.CheckUsed();
					if (!source.Used)
					{
						source.Start(buffer, volume, loops);
						return source;
					}
				}
			}
			else
			{
				foreach (var source in GameSources)
				{
					source.CheckUsed();
					if (!source.Used)
					{
						source.Start(buffer, volume, loops);
						return source;
					}
				}
			}

			return null;
		}

		public void Stop(bool game)
		{
			if (!initialized)
				return;

			if (!game)
			{
				foreach (var source in Sources)
					source.Stop();
			}

			foreach (var source in GameSources)
				source.Stop();
		}

		public void Pause(bool pause, bool game)
		{
			if (!initialized)
				return;

			if (!game)
			{
				foreach (var source in Sources)
					source.Pause(pause);
			}

			foreach (var source in GameSources)
				source.Pause(pause);
		}

		public void Dispose()
		{
			if (!initialized)
				return;

			context.Dispose();

			foreach (var source in Sources)
				source.Dispose();

			foreach (var source in GameSources)
				source.Dispose();
		}
	}
}
