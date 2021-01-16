using OpenTK.Audio.OpenAL;

namespace WarriorsSnuggery.Audio
{
	public class AudioDevice
	{
		const int miscSourceCount = 4;
		const int gameSourceCount = 60;

		public readonly ALDevice device;
		public readonly ALContext context;
		public readonly MusicAudioSource MusicSource;
		public readonly GameAudioSource[] MiscSources;
		public readonly GameAudioSource[] GameSources;
		readonly bool initialized;

		public AudioDevice()
		{
			device = ALC.OpenDevice(string.Empty);

			if (device.Equals(ALDevice.Null))
				return;

			context = ALC.CreateContext(device, new ALContextAttributes());
			ALC.MakeContextCurrent(context);

			var error = AL.GetError();
			if (error != ALError.NoError)
				throw new FailingSoundDeviceException(string.Format("Failed to open audio device. Error code: {0}.", error));

			MusicSource = new MusicAudioSource();

			MiscSources = new GameAudioSource[miscSourceCount];
			for (int i = 0; i < miscSourceCount; i++)
				MiscSources[i] = new GameAudioSource();

			GameSources = new GameAudioSource[gameSourceCount];
			for (int i = 0; i < gameSourceCount; i++)
				GameSources[i] = new GameAudioSource();

			initialized = true;
		}

		public GameAudioSource Play(GameAudioBuffer buffer, bool inGame, float volume, float pitch, Vector position, bool loops)
		{
			var source = Find(inGame);
			if (source == null)
				return null;

			source.SetPosition(position);
			source.SetVolume(volume, (inGame ? Settings.EffectsVolume : 1) * Settings.MasterVolume);
			source.SetPitch(pitch);
			source.Start(buffer, loops);
			return source;
		}

		public GameAudioSource Find(bool inGame)
		{
			if (!initialized)
				return null;

			var sourcesToUse = inGame ? GameSources : MiscSources;
			foreach (var source in sourcesToUse)
			{
				if (source.IsUsed())
					continue;

				return source;
			}

			return null;
		}

		public void Stop(bool game)
		{
			if (!initialized)
				return;

			if (!game)
			{
				foreach (var source in MiscSources)
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
				foreach (var source in MiscSources)
					source.Pause(pause);
			}

			foreach (var source in GameSources)
				source.Pause(pause);
		}

		public void Dispose()
		{
			if (!initialized)
				return;

			foreach (var source in MiscSources)
				source.Dispose();

			foreach (var source in GameSources)
				source.Dispose();

			ALC.DestroyContext(context);
			ALC.CloseDevice(device);
		}
	}
}
