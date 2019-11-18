using System.Collections.Generic;

namespace WarriorsSnuggery.Audio
{
	public static class AudioManager
	{
		static readonly Dictionary<string, AudioBuffer> buffers = new Dictionary<string, AudioBuffer>();

		public static void PlaySound(string name)
		{
			AudioController.Play(buffers[name], false, Settings.EffectsVolume, false);
		}

		public static AudioBuffer GetBuffer(string name)
		{
			return buffers[name];
		}

		public static void LoadSound(string name, string path)
		{
			if (buffers.ContainsKey(name))
				return;

			buffers.Add(name, new AudioBuffer(path + name + ".wav"));
		}

		public static void Dispose()
		{
			foreach (var buffer in buffers.Values)
			{
				buffer.Dispose();
			}
			buffers.Clear();
		}
	}
}
