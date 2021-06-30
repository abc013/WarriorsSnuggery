namespace WarriorsSnuggery.Audio
{
	public static class AudioController
	{
		static AudioDevice device;

		public static MusicAudioSource MusicSource => device.MusicSource;

		public static void Load()
		{
			device = new AudioDevice();
		}

		internal static GameAudioSource Play(GameAudioBuffer buffer, bool inGame, float volume, float pitch, Vector position, bool loops)
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
			AudioManager.Dispose();
		}
	}
}
