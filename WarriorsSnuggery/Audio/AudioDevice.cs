using OpenTK.Audio.OpenAL;
using OpenTK.Audio;

namespace WarriorsSnuggery.Audio
{
	public class AudioDevice
	{
		public readonly AudioSource[] Sources;
		public readonly AudioSource[] GameSources;
		readonly AudioContext context;
		public AudioDevice()
		{
			var device = AudioContext.DefaultDevice;
			context = new AudioContext(device);
			context.MakeCurrent();
			var error = AL.GetError();
			if (error != ALError.NoError)
				throw new FailingSoundDeviceException(string.Format("Failed to open audio device. Error code: {0}.", error));

			Sources = new AudioSource[4];
			for (int i = 0; i < 4; i++)
			{
				Sources[i] = new AudioSource();
			}

			GameSources = new AudioSource[12];
			for (int i = 0; i < 12; i++)
			{
				GameSources[i] = new AudioSource();
			}
		}

		public AudioSource Play(AudioBuffer buffer, bool inGame, float volume, bool loops)
		{
			if (!inGame)
			{
				foreach(var source in Sources)
				{
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
			if (!game)
			{
				foreach(var source in Sources)
				{
					source.Stop();
				}
			}

			foreach (var source in GameSources)
			{
				source.Stop();
			}
		}

		public void Pause(bool pause, bool game)
		{
			if (!game)
			{
				foreach (var source in Sources)
				{
					source.Pause(pause);
				}
			}

			foreach (var source in GameSources)
			{
				source.Pause(pause);
			}
		}

		public void Dispose()
		{
			context.Dispose();

			foreach (var source in Sources)
			{
				source.Dispose();
			}

			foreach (var source in GameSources)
			{
				source.Dispose();
			}
		}
	}
}
