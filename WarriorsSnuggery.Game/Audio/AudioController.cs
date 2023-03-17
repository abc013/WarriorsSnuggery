using WarriorsSnuggery.Audio.Music;
using WarriorsSnuggery.Audio.Sound;

namespace WarriorsSnuggery.Audio
{
	public static class AudioController
	{
		static AudioDevice device;

		public static MusicAudioSource MusicSource => device.MusicSource;
		public static MusicAudioSource IntenseMusicSource => device.IntenseMusicSource;

		public static void Load()
		{
			device = new AudioDevice();
		}

		internal static SoundAudioSource Play(SoundAudioBuffer buffer, bool inGame, float volume, float pitch, Vector position, bool loops)
		{
			return device.Play(buffer, inGame, volume, pitch, position, loops);
		}

		public static void StopAll(bool onlyInGame)
		{
			device.Stop(onlyInGame);
		}

		public static void PauseAll(bool pause, bool onlyInGame)
		{
			device.Pause(pause, onlyInGame);
		}

		public static void Dispose()
		{
			device.Dispose();
			SoundController.Dispose();
		}
	}
}
