using OpenTK.Audio.OpenAL;
using WarriorsSnuggery.Audio.Music;
using WarriorsSnuggery.Audio.Sound;

namespace WarriorsSnuggery.Audio
{
	public class AudioDevice
	{
		const int miscSourceCount = 4;
		const int gameSourceCount = 60;

		public readonly ALDevice device;
		public readonly ALContext context;
		public readonly MusicAudioSource MusicSource;
		public readonly MusicAudioSource IntenseMusicSource;
		public readonly SoundAudioSource[] MiscSources;
		public readonly SoundAudioSource[] GameSources;
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
				throw new FailingSoundDeviceException($"Failed to open audio device. Error code: {error}.");

			MusicSource = new MusicAudioSource();
			IntenseMusicSource = new MusicAudioSource();

			MiscSources = new SoundAudioSource[miscSourceCount];
			for (int i = 0; i < miscSourceCount; i++)
				MiscSources[i] = new SoundAudioSource();

			GameSources = new SoundAudioSource[gameSourceCount];
			for (int i = 0; i < gameSourceCount; i++)
				GameSources[i] = new SoundAudioSource();

			initialized = true;
		}

		public SoundAudioSource Play(SoundAudioBuffer buffer, bool inGame, float volume, float pitch, Vector position, bool loops)
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

		public SoundAudioSource Find(bool inGame)
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

		public void Stop(bool onlyInGame)
		{
			if (!initialized)
				return;

			if (!onlyInGame)
			{
				foreach (var source in MiscSources)
					source.Stop();
			}

			foreach (var source in GameSources)
				source.Stop();
		}

		public void Pause(bool pause, bool onlyInGame)
		{
			if (!initialized)
				return;

			if (!onlyInGame)
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
