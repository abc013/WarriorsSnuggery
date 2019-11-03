using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarriorsSnuggery.Audio;

namespace WarriorsSnuggery
{
	public static class AudioController
	{
		static AudioDevice device;

		public static void Load()
		{
			device = new AudioDevice();
			AudioManager.LoadSound("test", FileExplorer.FindPath(FileExplorer.Misc, "test", ".wav"));
			AudioManager.LoadSound("new1", FileExplorer.FindPath(FileExplorer.Misc, "new1", ".wav"));
		}

		public static void Tick()
		{
			device.Tick();
		}

		public static void Play(AudioBuffer buffer, bool inGame, bool loops)
		{
			device.Play(buffer, inGame, loops);
		}

		public static void Stop(bool game)
		{
			device.Stop(game);
		}

		public static void Pause(bool pause, bool game)
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
