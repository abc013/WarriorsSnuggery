using WarriorsSnuggery.Audio;

namespace WarriorsSnuggery
{
	public static class AudioController
	{
		static AudioDevice device;
		public static MusicController Music;

		public static void Load()
		{
			device = new AudioDevice();
			AudioManager.LoadSound("test", FileExplorer.FindPath(FileExplorer.Misc, "test", ".wav"));
			Music = new MusicController(FileExplorer.FilesIn(FileExplorer.Misc + @"music\", ".wav"));
		}

		public static void Tick()
		{
			Music.Tick();
		}

		public static AudioSource Play(AudioBuffer buffer, bool inGame, float volume, float pitch, Vector position, bool loops)
		{
			return device.Play(buffer, inGame, volume, pitch, position, loops);
		}

		public static void StopAll(bool game)
		{
			device.Stop(game);
		}

		public static void PauseAll(bool pause, bool game)
		{
			device.Pause(pause, game);
		}

		public static void Exit()
		{
			device.Dispose();
			AudioManager.Dispose();
		}
	}
}
