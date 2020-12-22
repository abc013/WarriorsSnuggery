using System.Collections.Generic;

namespace WarriorsSnuggery.Audio
{
	public static class AudioManager
	{
		static readonly Dictionary<string, GameAudioBuffer> buffers = new Dictionary<string, GameAudioBuffer>();

		public static GameAudioSource PlaySound(string name)
		{
			return AudioController.Play(buffers[name], false, 1f, 1f, Vector.Zero, false);
		}

		public static GameAudioSource PlaySound(string name, bool inGame, float volume, float pitch, Vector position, bool loops = false)
		{
			return AudioController.Play(buffers[name], inGame, volume, pitch, position, loops);
		}

		public static GameAudioBuffer GetBuffer(string name)
		{
			return buffers[name];
		}

		public static void LoadSound(string name, string path)
		{
			if (buffers.ContainsKey(name))
				return;

			buffers.Add(name, new GameAudioBuffer(path + name + ".wav"));
		}

		public static void Dispose()
		{
			foreach (var buffer in buffers.Values)
				buffer.Dispose();

			buffers.Clear();
		}
	}
}
