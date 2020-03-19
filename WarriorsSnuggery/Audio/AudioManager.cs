using System.Collections.Generic;

namespace WarriorsSnuggery.Audio
{
	public static class AudioManager
	{
		static readonly Dictionary<string, AudioBuffer> buffers = new Dictionary<string, AudioBuffer>();

		public static AudioSource PlaySound(string name)
		{
			return AudioController.Play(buffers[name], false, 1f, 1f, Vector.Zero, false);
		}

		public static AudioSource PlaySound(string name, bool inGame, float volume, float pitch, Vector position, bool loops = false)
		{
			return AudioController.Play(buffers[name], inGame, volume, pitch, position, loops);
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
				buffer.Dispose();

			buffers.Clear();
		}
	}
}
